using System;
using System.Globalization;
using System.Windows.Data;

namespace Exercise_4.Converters
{
    public class VisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool check = (bool)value;
            return check ? "Visible" : "Hidden";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string check = (string)value;
            return check.Equals("Visible")? true : false;
        }
    }
}