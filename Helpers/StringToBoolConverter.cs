using System;
using System.Windows.Data;

namespace TransliteratorWPF_Version.Helpers
{
    // TODO: Rewrite to BoolToStringConverter
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string stringValue = value.ToString();

            if (stringValue == "On")
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool boolValue = (bool)value;

            if (boolValue)
            {
                return "On";
            }
            return "Off";
        }
    }
}