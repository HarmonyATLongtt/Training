using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TreeViewProject.Converter
{
    internal class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    // Nếu path đã là absolute
                    if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
                        return new Uri(path, UriKind.Absolute);

                    // Nếu path là relative (ví dụ "Images/animated.gif")
                    return new Uri(path, UriKind.RelativeOrAbsolute);
                }
                catch
                {
                    return null!;
                }
            }
            return null!;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
}