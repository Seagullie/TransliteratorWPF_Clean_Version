using System;
using System.Windows.Data;

namespace TransliteratorWPF_Version.Helpers
{
    // TODO: Delete this class, create BoolToStateStringConverter
    internal class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = (string)value;

            if (val == "On") return "true";
            return "false";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}