using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ClassLibrary2.UI.ViewModel
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

        public virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private string _myName;

        public string MyName
        {
            get { return _myName; }
            set
            {
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

        private string _filePath;

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                RaisePropertyChange(nameof(FilePath));
            }
        }

        // gán giá trị và bổ sung bắt sự kiện thay đổi của giá trị bingding của itemselected (listbox) và itemsource (datagrid)
        private DataTable _TableSelected;

        public DataTable TableSelected
        {
            get => _TableSelected;
            set
            {
                _TableSelected = value;
                RaisePropertyChange(nameof(TableSelected));
            }
        }

        public ICommand updateCommand { get; set; }
        public ICommand LoadCommand { get; set; }

        public MainViewModel()
        {
            updateCommand = new RelayCommand(UpdateName);
            LoadCommand = new RelayCommand(LoadCommandInvoke);
        }

        private void LoadCommandInvoke()
        {
            try
            {
                FilePath = GetMdbFilePath();
                if (!string.IsNullOrEmpty(FilePath))
                {
                    var tables = LoadMdbFile(FilePath);
                    Tables = new ObservableCollection<DataTable>(tables);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace.ToString());
            }
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

                if (i == 2)
                {
                    table.Columns.Add("Ten");
                    table.Columns.Add("Lop");
                    table.Rows.Add("Hoang", "Huy");
                    table.Rows.Add("63TH1", "K3");
                }
                tables.Add(table);
            }
            return tables;
        }

        #region Load .mdb file

        private List<DataTable> LoadMdbFile(string filePath)
        {
            List<DataTable> tables = new List<DataTable>();
            if (!string.IsNullOrEmpty(filePath))
            {
                //string connectionString = "Provider=Microsoft.JET.OLEDB.4.0;"
                string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;"
                                        + "data source="
                                        + filePath + ";";
                
                //+ ";Password=";
                using (var connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    List<string> names = GetTableNames(connection);
                    if (names?.Count > 0)
                    {
                        foreach (string name in names)
                        {
                            var query = "SELECT * FROM `" + name + "`";
                            DataTable table = new DataTable(name);

                            using (var command = new OleDbCommand(query, connection))
                            {
                                using (var reader = command.ExecuteReader())
                                {
                                    // add columns
                                    for (int i = 0; i < reader.FieldCount; i++)
                                        table.Columns.Add(reader.GetName(i));

                                    //add rows
                                    while (reader.Read())
                                    {
                                        var row = table.NewRow();
                                        for (int i = 0; i < reader.FieldCount; i++)
                                            row[i] = reader[i].ToString();
                                        table.Rows.Add(row);
                                    }
                                }
                            }
                            tables.Add(table);
                        }
                        //tables.Add(ReadTable(connection, name));
                    }
                }
            }
            return tables;
        }

        private DataTable ReadTable(OleDbConnection connection, string name)
        {
            var query = "Select * From " + name;
            DataTable table = new DataTable();

            using (var command = new OleDbCommand(query, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    // add columns
                    for (int i = 0; i < reader.FieldCount; i++)
                        table.Columns.Add();

                    //add rows
                    while (reader.Read())
                    {
                        var row = table.NewRow();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row.ItemArray[i] = reader[i];
                        table.Rows.Add(row);
                    }
                }
            }
            return table;
        }

        private List<string> GetTableNames(OleDbConnection connection)
        {
            object[] restrictions = new[] { null, null, null, "TABLE" };
            DataTable namesTable = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, restrictions);
            return namesTable.Rows
                   .Cast<DataRow>()
                   .Select(row => row["TABLE_NAME"].ToString())
                   .ToList();
        }

        private string GetMdbFilePath()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "(mdb file)|*.mdb";
            dlg.CheckFileExists = true;
            dlg.Title = "load mdb file";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == true)
                return dlg.FileName;
            return string.Empty;
        }

        #endregion Load .mdb file
    }

    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action _a;

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