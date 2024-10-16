using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Windows.Input;
using LicenseContext = OfficeOpenXml.LicenseContext;
using System.Windows;
using System.Collections.ObjectModel;

namespace WPF_Ex
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _excelFilePath;
        private DataTable _displayedData;
        
        //private string _selectedSheet;
        private Dictionary<string, DataTable> _sheetData; //lưu trữ dữ liệu từ các sheet
        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _sheetNames = new ObservableCollection<string>();
            _sheetData = new Dictionary<string, DataTable>();
            LoadCommand = new RelayCommand(LoadExcelFile);
            ExportCommand = new RelayCommand(ExportToExcel);

            // Initialize EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private ObservableCollection<string> _sheetNames;
        public ObservableCollection<string> SheetNames
        {
            get { return _sheetNames; }
            set
            {
                _sheetNames = value;
                OnPropertyChanged(nameof(SheetNames));
            }
        }
        private string _selectedSheet;
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
                    LoadSheetData(_selectedSheet);  // Load dữ liệu của sheet đã chọn
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
                FileName = openFileDialog.FileName;
                _sheetData.Clear();

                using (var package = new ExcelPackage(new FileInfo(_excelFilePath)))
                {
                    SheetNames = new ObservableCollection<string>();
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
            bool hasHeader = true;  // Giả định là có header

            try
            {
                // Lấy số lượng hàng và cột từ sheet
                int totalRows = worksheet.Dimension.End.Row;
                int totalCols = worksheet.Dimension.End.Column;

                // Thêm các cột vào DataTable dựa trên hàng đầu tiên (header)
                for (int col = 1; col <= totalCols; col++)
                {
                    var columnName = hasHeader ? worksheet.Cells[1, col].Text : $"Column {col}";
                    if (string.IsNullOrWhiteSpace(columnName))
                    {
                        columnName = $"Column {col}";
                    }
                    dataTable.Columns.Add(columnName);
                }

               
                int startRow = hasHeader ? 2 : 1;

                // Lấy toàn bộ bảng dữ liệu từ sheet và thêm vào DataTable
                var dataRange = worksheet.Cells[startRow, 1, totalRows, totalCols];

                
                for (int rowNum = startRow; rowNum <= totalRows; rowNum++)
                {
                    var newRow = dataTable.NewRow();
                    for (int col = 0; col < totalCols; col++)
                    {
                        newRow[col] = worksheet.Cells[rowNum, col + 1].Text;  
                    }
                    dataTable.Rows.Add(newRow);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading Excel data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
