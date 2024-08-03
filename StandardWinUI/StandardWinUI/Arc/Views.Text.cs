// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1204 // Static elements should appear before instance elements
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1649 // File name should match first type name

namespace Arc.Views;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public class C4Extension : MarkupExtension
{ // Text-based C4 markup extension. GUI thread only.
    public string Source { get; set; } = string.Empty;

    public C4Extension()
    {
    }

    /// <inheritdoc/>
    protected override object ProvideValue(IXamlServiceProvider serviceProvider)
    {
        var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

        if (target?.TargetObject is not null)
        {
            if (target.TargetObject.GetType().FullName == "System.Windows.SharedDp")
            {
                return this.Source;
            }

            if (target.TargetProperty is not null)
            { // Add ExtensionObject (used in C4Update).
                C4Updater.C4AddExtensionObject(target.TargetObject, target.TargetProperty, this.Source);
            }
        }

        return HashedString.GetOrIdentifier(this.Source);
    }
}

[MarkupExtensionReturnType(ReturnType = typeof(BindingBase))]
public class C4BindingExtension : MarkupExtension
{ // Binding-based C4 markup extension. GUI thread only.
    public string Source { get; set; } = string.Empty;

    public C4BindingExtension()
    {
    }

    protected override object ProvideValue(IXamlServiceProvider serviceProvider)
    {
        var b = new Binding() { Path = new("Value"), Source = new C4BindingSource(this.Source), };
        return b;
    }
}

public class C4BindingSource : INotifyPropertyChanged
{
    public C4BindingSource(string key)
    {
        this.key = key;
        C4Updater.C4AddExtensionObject(this, null, null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public object? Value => HashedString.GetOrIdentifier(this.key);

    public void CultureChanged()
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
    }

    private string key;
}

/*[ContentProperty(nameof(Bindings))]
public class FormatExtension : IMarkupExtension<BindingBase>
{
    private static IMultiValueConverter converter = new BoundFormatConverter();

    public IList<BindingBase> Bindings
    {
        get => this.bindings ??= new List<BindingBase>();
        set => this.bindings = value;
    }

    public FormatExtension()
    {
    }

    public BindingBase ProvideValue(IServiceProvider serviceProvider)
    {
        var mb = new MultiBinding() { Mode = BindingMode.OneWay };
        mb.Converter = converter;
        foreach (var x in this.Bindings)
        {
            mb.Bindings.Add(x);
        }

        return mb;
    }

    object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        => (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);

    public class BoundFormatConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0)
            {
                throw new ArgumentException("values must have at least one element", "parameter");
            }

            var format = values[0].ToString();
            if (format == null)
            {
                return string.Empty;
            }

            try
            {
                switch (values.Length)
                {
                    case 1:
                        return format;
                    case 2:
                        return string.Format(format, values[1]);
                    case 3:
                        return string.Format(format, values[1], values[2]);
                    case 4:
                        return string.Format(format, values[1], values[2], values[3]);
                    default:
                        return string.Format(format, values.Skip(1).ToArray());
                }
            }
            catch (FormatException)
            {
                return "[FormatError]" + format;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    private IList<BindingBase>? bindings;
}*/

public class GCCountChecker
{ // カウンタ付きガーベージコレクション差分チェック。カウンタが一定以上になった場合、ガーベージコレクションのカウンタをチェックし、カウンタが変更されていたら、trueを返す。
    public GCCountChecker(int maxCount = 0)
    {// maxCount毎にガーベージコレクションのカウンタをチェックする。
        this.Count = 0;
        this.MaxCount = maxCount;
        this.PreviousCount = 0;
    }

    public int Count { get; private set; } // カウント

    public int MaxCount { get; } // カウント上限

    private int PreviousCount { get; set; } // ガーベージコレクションの前回のカウンタ

    public bool Check()
    { // カウンタが一定以上になった場合、ガーベージコレクションのカウンタをチェックし、カウンタが変更されていたら、trueを返す。
        this.Count++;
        if (this.Count >= this.MaxCount)
        {
            this.Count = 0;
            int x = GC.CollectionCount(0);
            if (x != this.PreviousCount)
            {
                this.PreviousCount = x;
                return true;
            }
        }

        return false;
    }
}

public static class C4Updater
{ // toolset
    private static object extensionObjectsCS = new object(); // 同期オブジェクト
    private static LinkedList<C4ExtensionObject> extensionObjects = new LinkedList<C4ExtensionObject>();
    private static GCCountChecker extensionObjectChecker = new GCCountChecker(16); // 16回に1回の頻度でチェック（使用されなくなったオブジェクトを解放する）。

    // C4ExtensionObject: C4Extensionのオブジェクトの更新用
    public class C4ExtensionObject
    {
        public WeakReference TargetObject; // target object or C4BindingSource
        public object? TargetProperty; // valid: target object, null: C4BindingSource
        public string? Key;

        public C4ExtensionObject(WeakReference targetObject, object? targetProperty, string? key)
        {
            this.TargetObject = targetObject;
            this.TargetProperty = targetProperty;
            this.Key = key;
        }
    }

    public static void C4AddExtensionObject(object targetObject, object? targetProperty, string? key)
    { // ExtensionObjectを追加する。マークアップ拡張から呼ばれる。
        lock (extensionObjectsCS)
        {
            extensionObjects.AddLast(new C4ExtensionObject(new WeakReference(targetObject), targetProperty, key));
            if (extensionObjectChecker.Check())
            {
                C4Clean();
            }
        }
    }

    public static void Update()
    { // C4を更新する。
        StandardWinUI.App.TryEnqueueOnUI(() =>
        {
            // GC.Collect();
            lock (extensionObjectsCS)
            {
                foreach (var x in extensionObjects)
                {
                    object? target = x.TargetObject?.Target;
                    if (target != null)
                    {
                        if (x.TargetProperty != null)
                        { // target object
                            if (x.TargetProperty is DependencyProperty)
                            {
                                DependencyObject? obj = target as DependencyObject;
                                DependencyProperty? prop = x.TargetProperty as DependencyProperty;
                                if (obj != null && x.Key != null)
                                {
                                    obj.SetValue(prop, HashedString.GetOrIdentifier(x.Key));
                                }

                                /*Action updateAction = () =>
                                {
                                    if (obj != null && x.Key != null)
                                    {
                                        obj.SetValue(prop, HashedString.GetOrIdentifier(x.Key));
                                    }
                                };

                                // Check whether the target object can be accessed from the
                                // current thread, and use Dispatcher.Invoke if it can't
                                if (obj != null)
                                {
                                    if (obj.CheckAccess())
                                    {
                                        updateAction();
                                    }
                                    else
                                    {
                                        obj.Dispatcher.Invoke(updateAction);
                                    }
                                }*/
                            }
                            else
                            {
                                var prop = x.TargetProperty as System.Reflection.PropertyInfo;
                                if (prop != null && x.Key != null)
                                {
                                    prop.SetValue(target, HashedString.GetOrIdentifier(x.Key));
                                }

                                if (x.Key is not null)
                                {
                                    if (x.TargetProperty is ProvideValueTargetProperty targetProperty)
                                    {
                                        targetProperty.
                                    }
                                }
                            }
                        }
                        else
                        { // C4BindingSource
                            var s = (C4BindingSource)target;
                            s.CultureChanged();
                        }
                    }
                    else
                    { // no target
                    }
                }

                // C4Clean();
            }
        });
    }

    private static void C4Clean()
    { // 使用されていないオブジェクトを解放する。内部で使用。
        LinkedListNode<C4ExtensionObject>? x, y;
        x = extensionObjects.First;
        while (x != null)
        {
            y = x.Next;
            if (x.Value.TargetObject.Target == null)
            {
                /* if (x.Value.TargetProperty != null) gl.Trace("_C4Clean: removed (target object)");
                else gl.Trace("_C4Clean: removed (C4BindingSource)"); */
                extensionObjects.Remove(x);
            }

            x = y;
        }
    }
}
