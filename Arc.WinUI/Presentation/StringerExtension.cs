// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

#pragma warning disable SA1204 // Static elements should appear before instance elements
#pragma warning disable SA1401 // Fields should be private

namespace Arc.WinUI;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
public class StringerExtension : MarkupExtension
{ // Text-based Stringer markup extension. GUI thread only.
    public string Source { get; set; } = string.Empty;

    public StringerExtension()
    {
    }

    /// <inheritdoc/>
    protected override object ProvideValue(IXamlServiceProvider serviceProvider)
    {
        var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        if (target?.TargetObject is not null)
        {
            if (target.TargetProperty is not null)
            { // Add ExtensionObject (used in StringerUpdate).
                Stringer.Register(target.TargetObject, target.TargetProperty, this.Source);
            }
        }

        return HashedString.GetOrIdentifier(this.Source);
    }
}

[MarkupExtensionReturnType(ReturnType = typeof(BindingBase))]
public class StringerBindingExtension : MarkupExtension
{ // Binding-based Stringer markup extension. GUI thread only.
    public string Source { get; set; } = string.Empty;

    public StringerBindingExtension()
    {
    }

    protected override object ProvideValue(IXamlServiceProvider serviceProvider)
    {
        return new Binding()
        {
            Path = new("Value"),
            Source = new StringerBindingSource(this.Source),
        };
    }
}

public class StringerBindingSource : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public object? Value => HashedString.GetOrIdentifier(this.key);

    private string key;

    public StringerBindingSource(string key)
    {
        this.key = key;
        Stringer.Register(this, null, string.Empty);
    }

    public void CultureChanged()
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
    }
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
