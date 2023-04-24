using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ex_WPF.MainWindow;

namespace Ex_WPF
{
   public class Model
    {
        public ObservableCollection<Student> Students = new ObservableCollection<Student>();
        public string Test;
        public Model()
        {
            Students = new ObservableCollection<Student>()
            {
                new Student()
                {
                    ID="dsf",
            Name="sdfga",
            Age=123
                }
            };
        }
    }
}
