using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WPF.Model;
using Excel = Microsoft.Office.Interop.Excel;

namespace WPF.ViewModel
{
    public class MainViewModel
    {
        public ObservableCollection<Student> Students { get; set; }

        public ICommand ShowWindowCommand { get; set; }

        public MainViewModel() {
            Students = ;
            Excel.A
        }

    }
}
