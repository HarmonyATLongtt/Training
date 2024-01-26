using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WPF.Commands;
using WPF.Model;
using Excel = Microsoft.Office.Interop.Excel;

namespace WPF.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private Person _person = new Person();
        private ObservableCollection<Person> _students;
        private ICommand _SubmitCommand;

        public Person Person
        {
            get { return _person; }
            set
            {
                _person = value;
                NotifyPropertyChanged(nameof(Person));
            }
        }

        public ObservableCollection<Person> Students
        {
            get { return _students; }
            set
            {
                _students = value;
                NotifyPropertyChanged(nameof(Students));
            }
        }

        public ICommand SubmitCommand
        {
            get
            {
                if (_SubmitCommand == null)
                {
                    _SubmitCommand = new RelayCommand(param => this.Submit(), null);
                }
                return _SubmitCommand;
            }
        }

        public ICommand ShowWindowCommand { get; set; }
        public ICommand ImportCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public void LoadStudents()
        {
            Students.Add(new Person() { Id = 1, Name = "ABC", Age = 20, Address = "HN" });
            Students.Add(new Person() { Id = 2, Name = "ABC", Age = 20, Address = "HN" });
            Students.Add(new Person() { Id = 3, Name = "ABC", Age = 20, Address = "HN" });
            Students.Add(new Person() { Id = 4, Name = "ABC", Age = 20, Address = "HN" });
        }

        public MainViewModel()
        {

            Person = new Person();
            Students = new ObservableCollection<Person>();
            LoadStudents();
            Students.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Students_CollectionChanged);
        }

        void Students_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(Students));
        }
        private void Submit()
        {
            Students.Add(Person);
            Person = new Person();
        }
    }
}
