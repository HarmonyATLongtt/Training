using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BaiTapThem.View
{
    public class AgeConverter2 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime birthday)
            {
                DateTime today = DateTime.Now;
                int age = today.Year - birthday.Year;
                if (birthday > today.AddYears(-age))
                {
                    age--;
                }
                return age;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}