using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Windows.Input;
using WPF_Ex;
using LicenseContext = OfficeOpenXml.LicenseContext;
using System.Windows;

namespace WPF_Ex
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _excelFilePath;
        private DataTable _displayedData;
        private List<string> _sheetNames;
        private string _selectedSheet;
        private Dictionary<string, DataTable> _sheetData;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _sheetNames = new List<string>();
            _sheetData = new Dictionary<string, DataTable>();
            LoadCommand = new RelayCommand(LoadExcelFile);
            ExportCommand = new RelayCommand(ExportToExcel);

            // Initialize EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public List<string> SheetNames
        {
            get { return _sheetNames; }
            set
            {
                _sheetNames = value;
                OnPropertyChanged(nameof(SheetNames));
            }
        }

        public string SelectedSheet
        {
            get { return _selectedSheet; }
            set
            {
                if (_selectedSheet != value)
                {
                    SaveCurrentSheetData();
                    _selectedSheet = value;
                    OnPropertyChanged(nameof(SelectedSheet));
                    LoadSheetData(_selectedSheet);
                }
            }
        }

        public DataTable DisplayedData
        {
            get { return _displayedData; }
            set
            {
                _displayedData = value;
                OnPropertyChanged(nameof(DisplayedData));
            }
        }

        public ICommand LoadCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void LoadExcelFile(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _excelFilePath = openFileDialog.FileName;
                _sheetData.Clear();

                using (var package = new ExcelPackage(new FileInfo(_excelFilePath)))
                {
                    SheetNames = new List<string>();
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        SheetNames.Add(worksheet.Name);
                        var dataTable = LoadWorksheetToDataTable(worksheet);
                        _sheetData[worksheet.Name] = dataTable;
                    }

                    if (SheetNames.Count > 0)
                    {
                        SelectedSheet = SheetNames[0];
                    }
                }
            }
        }

        private DataTable LoadWorksheetToDataTable(ExcelWorksheet worksheet)
        {
            var dataTable = new DataTable();
            bool hasHeader = true;
            int startRow = hasHeader ? 2 : 1;

            foreach (var cell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
            {
                dataTable.Columns.Add(hasHeader ? cell.Text : $"Column {cell.Start.Column}");
            }

            for (int rowNum = startRow; rowNum <= worksheet.Dimension.End.Row; rowNum++)
            {
                var row = dataTable.NewRow();
                int colIndex = 0;
                foreach (var cell in worksheet.Cells[rowNum, 1, rowNum, worksheet.Dimension.End.Column])
                {
                    row[colIndex++] = cell.Text;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        private void LoadSheetData(string sheetName)
        {
            if (_sheetData.TryGetValue(sheetName, out DataTable? value))
            {
                DisplayedData = value;
                return;

            }
            DisplayedData.Clear();
        }

        private void SaveCurrentSheetData()
        {
            if (!string.IsNullOrEmpty(_selectedSheet) && DisplayedData != null)
            {
                _sheetData[_selectedSheet] = DisplayedData;
            }
        }

        private void ExportToExcel(object parameter)
        {
            SaveCurrentSheetData();

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var package = new ExcelPackage())
                {
                    foreach (var sheetName in _sheetNames)
                    {
                        var worksheet = package.Workbook.Worksheets.Add(sheetName);
                        var dataTable = _sheetData[sheetName];

                        for (int col = 0; col < dataTable.Columns.Count; col++)
                        {
                            worksheet.Cells[1, col + 1].Value = dataTable.Columns[col].ColumnName;
                        }

                        for (int row = 0; row < dataTable.Rows.Count; row++)
                        {
                            for (int col = 0; col < dataTable.Columns.Count; col++)
                            {
                                worksheet.Cells[row + 2, col + 1].Value = dataTable.Rows[row][col];
                            }
                        }
                    }

                    package.SaveAs(new FileInfo(saveFileDialog.FileName));
                }

                MessageBox.Show("Data exported successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
