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

        public ICommand LoadCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand CreateCommand { get; set; }

        public MainViewModel()
        {
            LoadCommand = new RelayCommand(LoadCommandInvoke);
            CancelCommand = new RelayCommand(CancelCommandInvoke);
            CreateCommand = new RelayCommand(CreateCommandInvoke);
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

        private void CancelCommandInvoke()
        {
        }

        private void CreateCommandInvoke()
        {
        }

        #region Load .mdb file

        private List<DataTable> LoadMdbFile(string filePath)
        {
            List<DataTable> tables = new List<DataTable>();
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
    }
}