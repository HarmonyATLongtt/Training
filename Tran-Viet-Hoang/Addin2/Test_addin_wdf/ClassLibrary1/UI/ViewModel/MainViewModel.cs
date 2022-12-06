using Autodesk.Revit.DB.Visual;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClassLibrary1.UI.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void RaisePropertyChange(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        private DataTable _datagrid;
        public DataTable datagrid
        { get { return _datagrid; } 
            set
            {
                DataTable datagrid1 = new DataTable();
                datagrid1.Columns.Add("Hoang");
                datagrid1.Columns.Add("Hoang1");
                datagrid1.Columns.Add("Hoang2");
                _datagrid = datagrid1;
            }
        }
       
        public MainViewModel()
        {
           datagrid = new DataTable();
        }   
        //    public string _name;
        //    public string Name
        //    {
        //        get { return _name; }
        //        set
        //        {
        //            _name = value;
        //            RaisePropertyChange(nameof(Name));
        //        }
        //    }
        //}
        //public class RelayCommand : ICommand
        //{
        //    public event EventHandler CanExecuteChanged;
        //    Action _a;

        //    public RelayCommand(Action a)
        //    {
        //        _a = a;
        //    }

        //    public bool CanExecute(object parameter)
        //    {
        //        return true;
        //    }

        //    public void Execute(object parameter)
        //    {
        //        if (_a != null)
        //            _a.Invoke();
        //    }
    }
}
