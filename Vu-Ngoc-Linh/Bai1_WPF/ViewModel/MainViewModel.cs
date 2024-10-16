
using System;
using System.Collections.Generic;
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
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace Bai1_WPF.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ImportData { get; set; }
        private DataTable _data;
        private string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }
        public DataTable Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }
        public MainViewModel()
        {
            ImportData = new RelayCommand(ImportFile, CanImportFile);
            ExportData = new RelayCommand(ExportFile, CanExportFile);

        }

        private void ExportFile(object obj)
        {
            string filePath = "";
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveDialog.FileName = "";
            if (saveDialog.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(saveDialog.FileName);

                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ExportData");

                    int startRow = 1;
                    int startCol = 1;

                    for (int col = 0; col < Data.Columns.Count; col++)
                    {
                        worksheet.Cells[startRow, startCol + col].Value = Data.Columns[col].ColumnName;
                    }

                    for (int row = 0; row < Data.Rows.Count; row++)
                    {
                        for (int col = 0; col < Data.Columns.Count; col++)
                        {
                            worksheet.Cells[startRow + row + 1, startCol + col].Value = Data.Rows[row][col];
                        }
                    }

                    package.Save();
                    MessageBox.Show("Export successfully.");
                    return;
                }

            }

        }

        private bool CanExportFile(object obj)
        {
            //return Data != null;
            return true;
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
            DataTable dt = new DataTable();
            FileInfo fileInfo = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                dt = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column,
                                    worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].ToDataTable();

            }
            Data = dt;
        }
        public ICommand ExportData { get; set; }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
