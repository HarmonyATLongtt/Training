using Bai2_WPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2_WPF.Model
{
    public class MainModel
    {
        public string FilePath { get; set; }
        public string SelectedSheet { get; set; }
        public ObservableCollection<string> SheetNames { get; set; }
        public ObservableCollection<Student> Students { get; set; }
        public ObservableCollection<Teacher> Teachers { get; set; }
        public ObservableCollection<Employee> Employees { get; set; }
        public ObservableCollection<object> CurrentObject { get; set; }
        public MainModel()
        {
            FilePath = string.Empty;
            //Datas = new ObservableCollection<DataTable>();
            Students = new ObservableCollection<Student>();
            Teachers = new ObservableCollection<Teacher>();
            Employees = new ObservableCollection<Employee>();
            CurrentObject = new ObservableCollection<object>();
            SheetNames = new ObservableCollection<string>();
            SelectedSheet = SheetNames.FirstOrDefault();
            //SelectedData = Datas.FirstOrDefault();
        }
    }
}
