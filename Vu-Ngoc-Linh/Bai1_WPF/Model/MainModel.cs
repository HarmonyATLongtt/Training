using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai1_WPF.Model
{
    public class MainModel
    {
        public string FilePath { get; set; }
        public DataTable SelectedData { get;set; }
        public ObservableCollection<DataTable> Datas { get;set; }

        public MainModel() 
        { 
            FilePath = string.Empty;
            Datas = new ObservableCollection<DataTable>();
            SelectedData = Datas.FirstOrDefault();
        }  
    }
}
