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
using WPF_Ex.Model;

namespace WPF_Ex
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private MainModel _mainModel;  // Model quản lý dữ liệu
        private string _selectedSheet; // Sheet hiện tại đang được chọn
        private DataTable _displayedData;  // Dữ liệu được hiển thị lên UI (DataGrid)

        public MainViewModel()
        {
            _mainModel = new MainModel();
            LoadCommand = new RelayCommand(LoadExcelFile);
            ExportCommand = new RelayCommand(ExportToExcel);

            // Initialize EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public string FileName
        {
            get { return _mainModel.FilePath; }
            set
            {
                _mainModel.FilePath = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public ObservableCollection<string> SheetNames
        {
            get { return _mainModel.SheetNames; }
            set
            {
                _mainModel.SheetNames = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Mở file Excel và load dữ liệu vào Model
        private void LoadExcelFile(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _mainModel.ClearData();
                FileName = openFileDialog.FileName;

                using (var package = new ExcelPackage(new FileInfo(FileName)))
                {
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        var dataTable = LoadWorksheetToDataTable(worksheet);
                        _mainModel.SaveSheetData(worksheet.Name, dataTable);
                        _mainModel.SheetNames.Add(worksheet.Name);
                    }
                    SelectedSheet = _mainModel.SheetNames.FirstOrDefault();  // Chọn sheet đầu tiên
                }
            }
        }

        // Lưu sheet hiện tại vào Model trước khi chuyển sheet mới
        private void SaveCurrentSheetData()
        {
            if (!string.IsNullOrEmpty(SelectedSheet) && DisplayedData != null)
            {
                _mainModel.SaveSheetData(SelectedSheet, DisplayedData);
            }
        }

        // Load dữ liệu của sheet đã chọn
        private void LoadSheetData(string sheetName)
        {
            DisplayedData = _mainModel.GetSheetData(sheetName);
        }

        // Xuất dữ liệu ra file Excel
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
                    foreach (var sheetName in _mainModel.SheetNames)
                    {
                        var worksheet = package.Workbook.Worksheets.Add(sheetName);
                        var dataTable = _mainModel.GetSheetData(sheetName);

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

        // Chuyển worksheet thành DataTable
        private DataTable LoadWorksheetToDataTable(ExcelWorksheet worksheet)
        {
            var dataTable = new DataTable();
            bool hasHeader = true;

            int totalRows = worksheet.Dimension.End.Row;
            int totalCols = worksheet.Dimension.End.Column;

            for (int col = 1; col <= totalCols; col++)
            {
                var columnName = hasHeader ? worksheet.Cells[1, col].Text : $"Column {col}";
                dataTable.Columns.Add(columnName);
            }

            int startRow = hasHeader ? 2 : 1;
            for (int row = startRow; row <= totalRows; row++)
            {
                var newRow = dataTable.NewRow();
                for (int col = 1; col <= totalCols; col++)
                {
                    newRow[col - 1] = worksheet.Cells[row, col].Text;
                }
                dataTable.Rows.Add(newRow);
            }

            return dataTable;
        }
    }

}
