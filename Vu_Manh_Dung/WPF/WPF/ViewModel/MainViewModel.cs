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
        private List<Person> person = new List<Person>();
        public ObservableCollection<Person> Students { get; set; }

        public ICommand ShowWindowCommand { get; set; }

        public void LoadPerson()
        {
            person.Add(new Person() { Id = 1, Name = "ABC", Age = 20, Address = "HN" });
            person.Add(new Person() { Id = 2, Name = "ABC", Age = 20, Address = "HN" });
            person.Add(new Person() { Id = 3, Name = "ABC", Age = 20, Address = "HN" });
            person.Add(new Person() { Id = 4, Name = "ABC", Age = 20, Address = "HN" });
        }

        public MainViewModel() {
            
            Excel.Application exApp = new Excel.Application();

        }

        public bool CanImportFile(object obj)
        {
            return true;
        }

        public void ImportFile(object obj)
        {

        }

    }
}
