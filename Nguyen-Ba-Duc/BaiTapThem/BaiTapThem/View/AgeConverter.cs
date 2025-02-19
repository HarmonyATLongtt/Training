using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BaiTapThem.View
{
    public class AgeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is DateTime NgaySinh && values[1] is bool isAgeSelected)
            {
                if (isAgeSelected)
                {
                    DateTime today = DateTime.Now;
                    int age = today.Year - NgaySinh.Year;
                    if (NgaySinh > today.AddYears(-age)) age--;
                    return age.ToString();
                }
                return NgaySinh.ToShortDateString();
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}