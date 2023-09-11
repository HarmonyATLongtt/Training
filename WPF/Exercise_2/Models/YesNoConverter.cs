using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Exercise_1.Models
{
    public class YesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool check = (bool) value;
            return check ? "Visible" : "Hidden";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string check = (string) value;
            return check.Equals("Visible")? true : false;
        }
    }
}
