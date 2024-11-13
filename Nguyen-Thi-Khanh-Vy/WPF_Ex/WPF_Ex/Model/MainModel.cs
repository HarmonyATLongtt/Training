using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.ObjectModel;
using System.Linq;  // To use FirstOrDefault()

namespace WPF_Ex.Model
{
    public class MainModel
    {
        public string FileName { get; set; }  // Đường dẫn file Excel
        public ObservableCollection<string> SheetNames { get; set; } //SheetNames
        public ObservableCollection<object> SheetDatas { get; set; } // Dữ liệu của các sheet CurrentObject
        public string SelectedSheetData { get; set; } // Dữ liệu của sheet đã chọn SelectedSheet
        public ObservableCollection<Student> Students { get; set; }
        public ObservableCollection<Teacher> Teachers { get; set; }
        public ObservableCollection<Employee> Employees { get; set; }
        public MainModel()
        {
            Students = new ObservableCollection<Student>();
            Teachers = new ObservableCollection<Teacher>();
            Employees = new ObservableCollection<Employee>();
            SheetDatas = new ObservableCollection<object>();
            SheetNames = new ObservableCollection<string>();
            SelectedSheetData = SheetNames.FirstOrDefault();
        }
    }
}
