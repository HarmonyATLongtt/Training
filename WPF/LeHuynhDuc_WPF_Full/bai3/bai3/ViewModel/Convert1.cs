using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace bai3.ViewModel
{
    public class Convert1 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataTable dt = value as DataTable;
            int row = dt.Rows.Count, col = dt.Columns.Count;
            if (row==0 && col==0)
                return null;
            else
            {
                string s = "A1:";
                char c = (char)(64 + col);
                row++;
                s += c + row.ToString();
                return s;
            }    
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
