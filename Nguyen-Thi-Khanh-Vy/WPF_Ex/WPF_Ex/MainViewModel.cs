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

        public string FileName
        {
            get { return _mainModel.FilePath; }
            set
            {
                _mainModel.FilePath = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public ObservableCollection<DataTable> SheetDatas
        {
            get => _mainModel.SheetDatas;
            set
            {
                _mainModel.SheetDatas = value;
                OnPropertyChanged(nameof(SheetDatas));
            }
        } // Dữ liệu của các sheet

        public DataTable SelectedSheetData
        {
            get => _mainModel.SelectedSheetData;
            set
            {
                _mainModel.SelectedSheetData = value;
                OnPropertyChanged(nameof(SelectedSheetData));
            }
        } // Dữ liệu của các sheet

        public MainViewModel()
        {
            _mainModel = new MainModel();
            LoadCommand = new RelayCommand(LoadExcelFile);
            ExportCommand = new RelayCommand(ExportToExcel);

            // Initialize EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
                SheetDatas.Clear();
                FileName = openFileDialog.FileName;

                using (var package = new ExcelPackage(new FileInfo(FileName)))
                {
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        var dim = worksheet.Dimension;
                        var dt = worksheet.Cells[dim.Start.Row, dim.Start.Column, dim.End.Row, dim.End.Column].ToDataTable();
                        dt.TableName = worksheet.Name;
                        SheetDatas.Add(dt);
                    }
                    SelectedSheetData = SheetDatas.FirstOrDefault();
                }
            }
        }

        // Xuất dữ liệu ra file Excel
        private void ExportToExcel(object parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var package = new ExcelPackage())
                {
                    foreach (var table in _mainModel.SheetDatas)
                    {
                        var worksheet = package.Workbook.Worksheets.Add(table.TableName);
                        worksheet.SelectedRange.LoadFromDataTable(table);
                    }

                    package.SaveAs(new FileInfo(saveFileDialog.FileName));
                }

                MessageBox.Show("Data exported successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}