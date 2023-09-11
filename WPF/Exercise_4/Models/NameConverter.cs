using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Exercise_1.Models
{
    public class NameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null)
            {
                return "⇒ "  + values[0].ToString() + " "  + values[1].ToString();
            }
            else
                return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            string[] values = null;
            if (value != null && values.ToString() != null)
            {
                return values = value.ToString().Split(' ');
            }
            else
            { return null; }
        }
    }
}
