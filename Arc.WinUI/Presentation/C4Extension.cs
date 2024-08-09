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
        var dp = serviceProvider.GetService(typeof(DependencyProperty)) as DependencyProperty;

        if (target?.TargetObject is not null)
        {
            if (target.TargetObject.GetType().FullName == "System.Windows.SharedDp")
            {
                return this.Source;
            }

            if (target.TargetProperty is not null)
            { // Add ExtensionObject (used in C4Update).
                Presentation.RegisterC4(target.TargetObject, target.TargetProperty, this.Source);
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
        Presentation.RegisterC4(this, null, string.Empty);
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
