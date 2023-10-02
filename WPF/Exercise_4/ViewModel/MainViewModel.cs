using ClosedXML.Excel;
using Exercise_4.Commands;
using Exercise_4.Models;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Exercise_4.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Create a hash set to check sheet name
        private readonly HashSet<string> _sheetName = new HashSet<string>()
        {
            "Student", "Teacher", "Employee"
        };

        private ObservableCollection<Data> _listData;

        public ObservableCollection<Data> ListData
        {
            get
            {
                return _listData;
            }
            set
            {
                _listData = value;
            }
        }

        private Data _selectedItem;

        public Data SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        public ICommand ImportFileCommand { get; set; }
        public ICommand ExportFileCommand { get; set; }

        // Execute method inport file
        private void ImportFile(object obj)
        {
            // Get file path
            string filePath = "";
            ListData.Clear();
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Excel|*.xlsx"
            };
            if (dialog.ShowDialog() == true)
            {
                filePath = dialog.FileName;
            }

            try
            {
                using (var wb = new XLWorkbook(filePath))
                {
                    bool checkExist = true;
                    foreach (var ws in wb.Worksheets)
                    {
                        DataTable data = new DataTable();
                        // Get table in sheet
                        data = ws.Table(0).AsNativeDataTable();

                        ListData.Add(new Data() { DataOfTable = data, Name = ws.Name });
                        if (!_sheetName.Contains(ws.Name))
                        {
                            checkExist = false;
                        }
                    }
                    if (!checkExist)
                    {
                        ListData.Clear();
                        SelectedItem = null;
                        MessageBox.Show("File is not in correct format!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    MessageBox.Show("Import file successful!", "Message", MessageBoxButton.OK);
                }
                SelectedItem = ListData[0];
            }
            catch
            {
                ListData.Clear();
                SelectedItem = null;
                MessageBox.Show("File is not in correct format!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportFile(object obj)
        {
            // Get file path
            string path = "";
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Excel|*.xlsx"
            };
            if (dialog.ShowDialog() == true)
            {
                path = dialog.FileName;
            }
            try
            {
                using (var wb = new XLWorkbook())
                {
                    foreach (var i in ListData)
                    {
                        // Set sheet name
                        var ws = wb.AddWorksheet(i.Name);
                        ws.ColumnWidth = 20;
                        // Insert table
                        ws.FirstCell().InsertTable(i.DataOfTable);
                    }
                    // Save file
                    wb.SaveAs(path);
                    MessageBox.Show("Export file successful...", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch { }
        }

        public MainViewModel()
        {
            _listData = new ObservableCollection<Data>();
            ImportFileCommand = new RelayCommand(ImportFile, (e) => true);
            ExportFileCommand = new RelayCommand(ExportFile, (e) => true);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}