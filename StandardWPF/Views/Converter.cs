// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using Application;

#pragma warning disable SA1649 // File name should match first type name

namespace StandardWPF.Views;

/// <summary>
/// Formats dates using the localized application format, hiding the default value.
/// </summary>
public class DateTimeToStringConverter : IValueConverter
{// DateTime to String
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value != null && value is DateTime)
        {
            var dt = (DateTime)value;
            if (dt.Ticks == 0)
            {
                return string.Empty;
            }

            return dt.ToString(HashedString.Get("App.DateTime"));
        }

        return System.Windows.DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts between a positive display scale and percentage text.
/// </summary>
public class DisplayScalingToStringConverter : IValueConverter
{// Display scaling to String
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is double d)
        {
            return (d * 100).ToString("F0", culture) + "%";
        }
#if XAMARIN
        return null;;
#else
        return System.Windows.DependencyProperty.UnsetValue;
#endif
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string st &&
            double.TryParse(st.TrimEnd('%', ' '), System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, culture, out var percent) &&
            double.IsFinite(percent) && percent > 0)
        {
            return percent / 100;
        }

        return System.Windows.DependencyProperty.UnsetValue;
    }
}

/// <summary>
/// Resolves supported culture codes to localized language names.
/// </summary>
public class CultureToStringConverter : IValueConverter
{// Culture to String
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value != null && value is string)
        {
            switch ((string)value)
            {
                case "en": // eglish
                    return HashedString.Get("Language.En");
                case "ja": // japanese
                    return HashedString.Get("Language.Ja");

                default: // default = english
                    return HashedString.Get("Language.En");
            }
        }
#if XAMARIN
        return null;;
#else
        return System.Windows.DependencyProperty.UnsetValue;
#endif
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts booleans to visibility, inverting the result when a parameter is supplied.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        bool b = value is true;

        if (parameter != null)
        { // Reverse conversion on any given parameter.
            b = !b;
        }

        return b ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        bool b;

        if (value is System.Windows.Visibility v)
        {
            b = v == System.Windows.Visibility.Visible;
        }
        else
        {
            b = false;
        }

        if (parameter != null)
        { // Reverse conversion on any given parameter.
            b = !b;
        }

        return b;
    }
}

/// <summary>
/// Negates boolean values, treating null and non-boolean values as false.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        bool b = value is true;

        return !b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        bool b;

        if (value is bool b2)
        {
            b = b2;
        }
        else
        {
            b = false;
        }

        return !b;
    }
}
