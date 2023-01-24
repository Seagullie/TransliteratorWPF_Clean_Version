using System;
using System.Windows.Data;

namespace TransliteratorWPF_Version.Helpers
{
    internal class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is bool)
            {
                return !((bool)value);
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is bool)
            {
                return !((bool)value);
            }
            return true;
        }
    }
}
