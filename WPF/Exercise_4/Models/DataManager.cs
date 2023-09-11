using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise_1.Models
{
    public class DataManager
    {
        public static ObservableCollection<Data> data = new ObservableCollection<Data>();
        public static ObservableCollection<Data> GetData() { return data; }
        public static void AddData(Data _data) { data.Add(_data); }
    }
}
