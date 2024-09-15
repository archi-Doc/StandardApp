// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Arc.WinUI.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool b = false;
        if (value is bool)
        {
            b = (bool)value;
        }
        else if (value is bool?)
        {
            var b2 = (bool?)value;
            b = b2.HasValue ? b2.Value : false;
        }

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

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool b = false;
        if (value is bool)
        {
            b = (bool)value;
        }
        else if (value is bool?)
        {
            var b2 = (bool?)value;
            b = b2.HasValue ? b2.Value : false;
        }

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
