using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Application = Autodesk.Revit.ApplicationServices.Application;
using Autodesk.Revit.UI.Selection;
using System.Windows;
using ClassLibrary2.UI.Views;
using System.Windows.Controls;

namespace ClassLibrary2.UI.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
       
        public void RaisePropertyChange(string propName)
        {
            if (PropertyChanged!= null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged!= null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        private string _myName;
        public string MyName 
        { 
            get { return _myName; } 
            set {
                _myName = value; 
                RaisePropertyChange(nameof(MyName));
            }
        }

        private int _myAge;
        public int MyAge
        {
            get { return _myAge; }
            set
            {
                _myAge = value;
                RaisePropertyChange(nameof(MyAge));
            }
        }

        //tao itemsource de bind vao listbox
        private ObservableCollection<DataTable> _tables;
        public ObservableCollection<DataTable> Tables 
        {
            get => _tables; 
            set
            {
                _tables = value;
                RaisePropertyChange(nameof(Tables));
            }
        }

        // gán giá trị và bổ sung bắt sự kiện thay đổi của giá trị bingding của itemselected (listbox) và itemsource (datagrid)
        private DataTable _TableSelected;
        public DataTable TableSelected
        {
            get => _TableSelected;
            set {
              
                _TableSelected = value ;
            RaisePropertyChange(nameof(TableSelected));
            }
        }


       
        public ICommand updateCommand { get; set; }

        public MainViewModel()
        {
           
            updateCommand = new RelayCommand(UpdateName);
            // Mặc định cho listbox hiện lên sẽ nhận có itemsource này
            _tables = new ObservableCollection<DataTable>(InitTables());      
            

        }

        private void UpdateName()
        {
            MyName = "Honag";

        }
  
        //tao du lieu cho item source cua listbox
        private IEnumerable<DataTable> InitTables()
        {
            List<DataTable> tables = new List<DataTable>();           
            for (int i = 1; i <= 10; i++)
            {
                DataTable table = new DataTable()
                {
                    TableName = "table_" + i.ToString(),              
                };
               
                if (i==2)
                {
                    table.Columns.Add("Ten");
                    table.Columns.Add("Lop");
                    table.Rows.Add("Hoang" ,"Huy");
                    table.Rows.Add("63TH1" , "K3" );
                }
                tables.Add(table);
            }
            return tables;         
        }



        //private ICollection<ElementId> _GetElementIds;
        //public ICollection<ElementId> GetElementIds
        //{
        //    get => _GetElementIds;
        //    set {

        //        _GetElementIds = value;
        //        RaisePropertyChange(nameof(GetElementIds));
        //        MessageBox.Show("Phần tử có ID là "+ _GetElementIds);
        //    }
        //}





    }

    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        Action _a;

        public RelayCommand(Action a)
        {
            _a = a;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_a != null)
                _a.Invoke();
        }
    }

    public class Doc : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            var collector = new FilteredElementCollector(doc).GetElementCount();
            return Result.Succeeded;
        }
    }
}
