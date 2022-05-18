using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF_Sample.Converters
{
    /// <summary>
    /// Convert strung to upper case
    /// </summary>
    internal class StringToUpperCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (value is not null)
            {
                return value.ToString().ToUpper();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}