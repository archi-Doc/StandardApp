using System.Globalization;
using System.Windows;
using Arc.WPF;
using StandardWPF.Views;

namespace StandardApp.Tests;

/// <summary>
/// Tests formatting cultures, invalid input, and boolean conversion.
/// </summary>
public class ConverterTests
{
    [Theory]
    [InlineData("en-US", "125.5%")]
    [InlineData("fr-FR", "125,5%")]
    public void DisplayScaleUsesBindingCulture(string cultureName, string text)
    {
        var converter = new DisplayScalingToStringConverter();
        Assert.Equal(1.255, converter.ConvertBack(text, typeof(double), null!, CultureInfo.GetCultureInfo(cultureName)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("0%")]
    [InlineData("-50%")]
    public void InvalidScaleReturnsUnsetValue(string text)
    {
        Assert.Same(DependencyProperty.UnsetValue, new DisplayScalingToStringConverter().ConvertBack(text, typeof(double), null!, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void BoundFormatUsesBindingCultureForEveryArity(int count)
    {
        var values = new object[count + 1];
        values[0] = "{0:F1}";
        Array.Fill(values, 1.5, 1, count);
        var converter = new FormatExtension.BoundFormatConverter();
        Assert.Equal("1,5", converter.Convert(values, typeof(string), null!, CultureInfo.GetCultureInfo("fr-FR")));
    }

    [Fact]
    public void BoundFormatHandlesMissingAndMalformedFormats()
    {
        var converter = new FormatExtension.BoundFormatConverter();
        Assert.Equal(string.Empty, converter.Convert(new object[] { null! }, typeof(string), null!, CultureInfo.InvariantCulture));
        Assert.Equal("[FormatError]{1}", converter.Convert(new object[] { "{1}", 5 }, typeof(string), null!, CultureInfo.InvariantCulture));
        Assert.Equal("values", Assert.Throws<ArgumentException>(() => converter.Convert([], typeof(string), null!, CultureInfo.InvariantCulture)).ParamName);
    }

    [Theory]
    [InlineData(true, Visibility.Visible)]
    [InlineData(false, Visibility.Collapsed)]
    [InlineData(null, Visibility.Collapsed)]
    public void BooleanVisibilityPreservesNullableAndInversionBehavior(bool? value, Visibility expected)
    {
        var converter = new BoolToVisibilityConverter();
        Assert.Equal(expected, converter.Convert(value!, typeof(Visibility), null!, CultureInfo.InvariantCulture));
        Assert.NotEqual(expected, converter.Convert(value!, typeof(Visibility), "invert", CultureInfo.InvariantCulture));
        Assert.Equal(!value.GetValueOrDefault(), new InverseBoolConverter().Convert(value!, typeof(bool), null!, CultureInfo.InvariantCulture));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [InlineData(null)]
    public void WinuiBooleanConvertersPreserveNullableAndRoundTripBehavior(bool? value)
    {
        var converter = new Arc.WinUI.Converters.BoolToVisibilityConverter();
        var expected = value == true ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
        Assert.Equal(expected, converter.Convert(value!, typeof(Microsoft.UI.Xaml.Visibility), null!, "en"));
        Assert.NotEqual(expected, converter.Convert(value!, typeof(Microsoft.UI.Xaml.Visibility), "invert", "en"));
        Assert.Equal(value.GetValueOrDefault(), converter.ConvertBack(expected, typeof(bool), null!, "en"));
        Assert.Equal(!value.GetValueOrDefault(), converter.ConvertBack(expected, typeof(bool), "invert", "en"));
        var inverse = new Arc.WinUI.Converters.InverseBoolConverter();
        Assert.Equal(!value.GetValueOrDefault(), inverse.Convert(value!, typeof(bool), null!, "en"));
        Assert.Equal(!value.GetValueOrDefault(), inverse.ConvertBack(value!, typeof(bool), null!, "en"));
    }
}
