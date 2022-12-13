using ClassLibrary2.Data;
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

namespace ClassLibrary2.UI.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public List<LevelData> LevelDatas { get; set; }

        public MainViewModel()
        {
            LoadCommand = new RelayCommand(LoadCommandInvoke);
            CreateCommand = new RelayCommand(CreateCommandInvoke);
            CloseCommand = new HelperCommand(UserClose, CanClose);
            //CreateCommand = new HelperCommand(ListboxTables, CanClose);
        }

        public ICommand CloseCommand { get; set; }

        public ICommand LoadCommand { get; set; }

        public ICommand CreateCommand { get; set; }

        // bắt sự kiện thay đổi properties của control
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
            set
            {
                _TableSelected = value;
                RaisePropertyChange(nameof(TableSelected));
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

        private void CreateCommandInvoke()
        {
            try
            {
                foreach (DataTable ta in Tables)
                {
                    switch (ta.TableName)
                    {
                        //case "Beam Object Connectivity":
                        //    MessageBox.Show(tab);
                        //    break;
                        //case "Column Object Connectivity":
                        //    MessageBox.Show(tab);
                        //    break;
                        //case "Concrete Beam Flexure Envelope - TCVN 5574-2012":
                        //    MessageBox.Show(tab);
                        //    break;
                        //case "Concrete Beam Shear Envelope - TCVN 5574-2012":
                        //    MessageBox.Show(tab);
                        //    break;
                        //case "Frame Assignments - Insertion Point":
                        //    MessageBox.Show(tab);
                        //    break;
                        //case "Frame Assignments - Section Properties":
                        //    MessageBox.Show(tab);
                        //    break;
                        //case "Frame Section Property Definitions - Concrete Beam Reinforcing":
                        //    MessageBox.Show(tab);
                        //    break;
                        //case "Frame Section Property Definitions - Concrete Rectangular":
                        //    MessageBox.Show(tab);
                        //    break;
                        case "Point Object Connectivity":                         
                            break;

                        case "Story Definitions":

                            //string Hoangcode = string.Empty;
                            //List<LevelData> LevelList = LevelReadData( ta);
                            //foreach (var singlestory in LevelList)
                            //{
                            //    Hoangcode += singlestory.Name + " có cote là " + singlestory.Elevation + "\n";
                            //}
                            //MessageBox.Show("Hoang" + "\n" + Hoangcode);

                            LevelDatas = LevelReadData(ta);

                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace.ToString());
            }
        }

        #region Level ReadData

        private List<LevelData> LevelReadData(DataTable table)
        {
            double elev = 0;
            var levelclass = new List<LevelData>();

            LevelData baseLevel = new LevelData();
            baseLevel.Elevation = elev;
            baseLevel.Name = "Base";
            levelclass.Add(baseLevel);

            var accendingRows = table.Rows
                                .Cast<DataRow>()
                                .OrderBy(r => r["Name"].ToString());
            foreach (DataRow row in accendingRows)
            {
                string height = row["Height"].ToString();
                if (double.TryParse(height, out double val))
                {
                    elev += val;

                    LevelData levelData = new LevelData();
                    levelData.Elevation = elev;
                    levelData.Name = row["Name"].ToString();

                    levelclass.Add(levelData);
                };
            }
            return levelclass;
        }

        #endregion Level ReadData

        #region Load .mdb file

        private List<DataTable> LoadMdbFile(string filePath)
        {
            List<DataTable> tables = new List<DataTable>();
            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    string connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;"
                                            + "data source="
                                            + filePath + ";";

                    using (var connection = new OleDbConnection(connectionString))
                    {
                        connection.Open();
                        List<string> tableNames = GetTableNames(connection);
                        if (tableNames?.Count > 0)
                        {
                            foreach (string tableName in tableNames)
                            {
                                var table = ReadTable(connection, tableName);
                                tables.Add(table);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace.ToString());
            }
            return tables;
        }

        private DataTable ReadTable(OleDbConnection connection, string tableName)
        {
            var query = "Select * From `" + tableName + "`";
            DataTable table = new DataTable(tableName);

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

        #region WindowClosed

        private void UserClose(object parameter)
        {
            Window wnd = parameter as Window;
            MessageBox.Show("Have a great day!!");
            wnd?.Close();
        }

        private bool CanClose(object parameter) => true;

        public class HelperCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;

            //public HelperCommand(Action<object> execute) : this(execute, canExecute: null) { }
            public HelperCommand(Action<object> execute, Predicate<object> canExecute)
            {
                if (execute == null) throw new ArgumentNullException("execute");
                this._execute = execute;
                this._canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) => this._canExecute == null ? true : this._canExecute(parameter);

            public void Execute(object parameter) => this._execute(parameter);

            public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion WindowClosed
    }
}