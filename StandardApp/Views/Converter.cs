// Copyright (c) All contributors. All rights reserved. Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using Application;

#pragma warning disable SA1649 // File name should match first type name

namespace StandardApp
{
    public class DisplayScalingToStringConverter : IValueConverter
    {// Display scaling to String
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double d)
            {
                return (d * 100).ToString("F0") + "%";
            }
#if XAMARIN
            return null;;
#else
            return System.Windows.DependencyProperty.UnsetValue;
#endif
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double d = 1;

            if (value is string st)
            {
                d = double.Parse(st.TrimEnd(new char[] { '%', ' ' })) / 100;
            }

            return d;
        }
    }

    public class CultureToStringConverter : IValueConverter
    {// Culture to String
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is string)
            {
                switch ((string)value)
                {
                    case "en": // eglish
                        return App.C4["language.en"];
                    case "ja": // japanese
                        return App.C4["language.ja"];

                    default: // default = english
                        return App.C4["language.en"];
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
}
