﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Bai1_WPF.Command;
using Bai1_WPF.Model;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace Bai1_WPF.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainModel Model { get; private set; }

        public string FilePath
        {
            get => Model.FilePath;
            set
            {
                Model.FilePath = value;
                OnPropertyChanged();
            }
        }

        public DataTable SelectedData
        {
            get => Model.SelectedData;
            set
            {
                Model.SelectedData = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DataTable> Datas
        {
            get => Model.Datas;
            set
            {
                Model.Datas = value;
                OnPropertyChanged();
            }
        }

        public ICommand ImportData { get; set; }
        public ICommand ExportData { get; set; }

        public MainViewModel(MainModel model)
        {
            Model = model;
            ImportData = new RelayCommand(ImportFile, CanImportFile);
            ExportData = new RelayCommand(ExportFile, CanExportFile);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CanImportFile(object obj)
        {
            return true;
        }

        private void ImportFile(object obj)
        {
            string filePath = "";
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = "Excel files (*.xlsx)|*.xlsx|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                filePath = dialog.FileName;
            }
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Invalid file.");
                return;
            }
            FilePath = filePath;
            FileInfo fileInfo = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                Datas.Clear();
                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    DataTable dt = new DataTable();
                    dt = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column,
                                    worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].ToDataTable();
                    dt.TableName = worksheet.Name;
                    Datas.Add(dt);
                }
                SelectedData = Datas.FirstOrDefault();
            }
        }

        private void ExportFile(object obj)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveDialog.FileName = "";
            if (saveDialog.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(saveDialog.FileName);
                using (ExcelPackage pkg = new ExcelPackage(fileInfo))
                {
                    foreach (DataTable dt in Datas)
                    {
                        ExcelWorksheet ws = pkg.Workbook.Worksheets.Add(dt.TableName);
                        ws.Cells["A1"].LoadFromDataTable(dt, true);
                    }
                    pkg.Save();
                    MessageBox.Show("Export successfully!");
                    return;
                }
            }
        }

        private bool CanExportFile(object obj)
        {
            return true;
        }
    }
}