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
        public Dictionary<string, DataTable> SheetData { get; set; } // Dữ liệu của các sheet
        public ObservableCollection<string> SheetNames { get; set; } // Tên các sheet trong file Excel

        public MainModel()
        {
            SheetNames = new ObservableCollection<string>();
            SheetData = new Dictionary<string, DataTable>();
        }

        // Lưu dữ liệu của một sheet
        public void SaveSheetData(string sheetName, DataTable dataTable)
        {
            if (SheetData.ContainsKey(sheetName))
            {
                SheetData[sheetName] = dataTable;
            }
            else
            {
                SheetData.Add(sheetName, dataTable);
            }
        }

        // Lấy dữ liệu của sheet
        public DataTable GetSheetData(string sheetName)
        {
            return SheetData.ContainsKey(sheetName) ? SheetData[sheetName] : new DataTable();
        }

        // Xóa dữ liệu
        public void ClearData()
        {
            SheetNames.Clear();
            SheetData.Clear();
        }
    }

}

