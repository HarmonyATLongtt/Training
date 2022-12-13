using ClassLibrary2.Data;
using ClassLibrary2.UI.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClassLibrary2.UI.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {

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
              string tab = "";
              string Levelinfo = "";
              double[] Levelelevalue = new double[100];
              string test1 = "";
              //double testele = -1;
              foreach(DataTable ta in Tables)
                {
                    tab = ta.TableName;
                    switch (tab)
                    {

                        case "Beam Object Connectivity":
                            MessageBox.Show(tab);
                            break;
                        case "Column Object Connectivity":
                            MessageBox.Show(tab);
                            break;
                        case "Concrete Beam Flexure Envelope - TCVN 5574-2012":
                            MessageBox.Show(tab);
                            break;
                        case "Concrete Beam Shear Envelope - TCVN 5574-2012":
                            MessageBox.Show(tab);
                            break;
                        case "Frame Assignments - Insertion Point":
                            MessageBox.Show(tab);
                            break;
                        case "Frame Assignments - Section Properties":
                            MessageBox.Show(tab);
                            break;
                        case "Frame Section Property Definitions - Concrete Beam Reinforcing":
                            MessageBox.Show(tab);
                            break;
                        case "Frame Section Property Definitions - Concrete Rectangular":
                            MessageBox.Show(tab);
                            break;
                        case "Point Object Connectivity":
                            MessageBox.Show(tab);
                            break;
                        case "Story Definitions":
                            LevelData[] Level = new LevelData[10];
                                                     
                            // lấy ra height và tính cao độ cho từng tầng
                            for (int i = 0; i < ta.Columns.Count; i++)
                                {                                
                                    if (ta.Columns[i].ColumnName == "Height")
                                    {
                                        for(int j = ta.Rows.Count-1; j >= 0 ; j--)
                                        {                                         
                                                int stt = ta.Rows.Count - j;
                                                int lowerstt =  stt -1;
                                                Double.TryParse(ta.Rows[j][ta.Columns[i]].ToString(), out double elevalue);
                                                Levelelevalue[0] = 0;
                                                Levelelevalue[stt] = elevalue + Levelelevalue[lowerstt];
                                                test1 += "Level "+(stt+1).ToString()+" có giá trị Elevation là: "+ Levelelevalue[stt].ToString() + "\n";
                                        }
                                    }
                                }                           
                                MessageBox.Show("Level 1 có giá trị Elevation là: " +Levelelevalue[0].ToString() + "\n" + test1);
                            
                            // lấy ra name và gắn cao độ (lấy được ở trên) của từng tầng cho name của tầng đó
                            for (int i = 0; i < ta.Rows.Count; i++)
                            {
                                for (int j = 0; j < ta.Columns.Count; j++)
                                {
                                    if (ta.Columns[j].ColumnName == "Name")
                                    {
                                        Level[i] = new LevelData();
                                        Level[i].Elevation = Levelelevalue[ta.Rows.Count - i];
                                        Level[i].Name = ta.Rows[i][ta.Columns[j]].ToString();                                     
                                        Levelinfo += Level[i].Name + "   " + Level[i].Elevation + "\n" ;
                                    }                                 
                                }
                            }
                            MessageBox.Show(tab);                          
                            MessageBox.Show(Levelinfo);
                            break;
                        default:
                        break;

                    }
                    //tab += ta.TableName + "\n";
              }
              //MessageBox.Show(tab);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace.ToString());
            }

        }

        //private void ListboxTables(object parameter)
        //{
        //    ListBox listdata = parameter as ListBox;
        //    DataTable modelimport = new DataTable();
        //    string tablenameslist = "";
        //    foreach( var lists in listdata.Items )
        //    {
        //        tablenameslist += lists.GetType().Name.ToString() +"\n";
        //    }

        //}

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
            catch(Exception ex)
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
            MessageBox.Show("Have a great day!!" );
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