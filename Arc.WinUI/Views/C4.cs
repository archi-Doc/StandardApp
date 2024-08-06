// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

#pragma warning disable SA1204 // Static elements should appear before instance elements
#pragma warning disable SA1401 // Fields should be private

namespace Arc.WinUI;

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
                C4.AddExtensionObject(target.TargetObject, target.TargetProperty, this.Source);
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
        C4.AddExtensionObject(this, null, null);
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

public static class C4
{
    private static object syncObject = new object(); // 同期オブジェクト
    private static LinkedList<ExtensionObject> extensionObjects = new LinkedList<ExtensionObject>();
    private static GCCountChecker extensionObjectChecker = new GCCountChecker(16); // 16回に1回の頻度でチェック（使用されなくなったオブジェクトを解放する）。

    private class ExtensionObject
    {
        public WeakReference TargetObject; // target object or C4BindingSource
        public object? TargetProperty; // valid: target object, null: C4BindingSource
        public string? Key;

        public ExtensionObject(WeakReference targetObject, object? targetProperty, string? key)
        {
            this.TargetObject = targetObject;
            this.TargetProperty = targetProperty;
            this.Key = key;
        }
    }

    public static void AddExtensionObject(object targetObject, object? targetProperty, string? key)
    {
        lock (syncObject)
        {
            extensionObjects.AddLast(new ExtensionObject(new WeakReference(targetObject), targetProperty, key));
            if (extensionObjectChecker.Check())
            {
                Clean();
            }
        }
    }

    /// <summary>
    /// Updates the display of C4.<br/>
    /// Please call from the UI thread.<br/>
    /// If not on the UI thread, consider using App.TryEnqueueOnUI().
    /// </summary>
    public static void Refresh()
    {
      // GC.Collect();
        lock (syncObject)
        {
            foreach (var x in extensionObjects)
            {
                if (x.Key is null)
                {
                    if (x.TargetObject?.Target is C4BindingSource c4BindingSource)
                    { // C4BindingSource
                        c4BindingSource.CultureChanged();
                    }

                    continue;
                }

                var target = x.TargetObject?.Target;
                if (target is TextBlock textBlock)
                {// TextBlock
                    textBlock.Text = HashedString.GetOrIdentifier(x.Key);
                }
            }

            // C4Clean();
        }
    }

    private static void Clean()
    {
        LinkedListNode<ExtensionObject>? x, y;
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
