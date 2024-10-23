using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Ex.Model
{
    public class MainModel
    {
        public string FilePath { get; set; }  // Đường dẫn file Excel
        public ObservableCollection<DataTable> SheetDatas { get; set; } // Dữ liệu của các sheet
        public DataTable SelectedSheetData { get; set; } // Dữ liệu của các sheet

        public MainModel()
        {
            SheetDatas = new ObservableCollection<DataTable>();
            SelectedSheetData = SheetDatas.FirstOrDefault();
        }
    }
}