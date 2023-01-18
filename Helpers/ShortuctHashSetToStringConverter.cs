using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace TransliteratorWPF_Version.Helpers
{
    public class ShortuctHashSetToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is HashSet<string> shortuct)
            {
                var valAsArray = shortuct.ToArray<string>();
                var valAsString = string.Join(" + ", valAsArray);
                return $"Toggle Shortcut: {valAsString}";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}