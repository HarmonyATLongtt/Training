using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF_Sample.Converters
{/// <summary>
/// Support revert boolean value from true -> false,  false -> true
/// </summary>
    internal class BooleanToReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked)
            {
                return !isChecked;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked)
            {
                return !isChecked;
            }
            return false;
        }
    }
}