// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Arc.WinUI.Converters;

/// <summary>
/// Converts booleans to visibility, inverting the result when a parameter is supplied.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool b = value is true;

        if (parameter != null)
        { // Reverse conversion on any given parameter.
            b = !b;
        }

        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        bool b;
        if (value is Visibility visibility)
        {
            b = visibility == Visibility.Visible;
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
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool b = value is true;

        return !b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
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
