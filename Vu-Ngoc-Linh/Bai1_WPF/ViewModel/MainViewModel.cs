using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
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
        public ICommand ImportData {  get; set; }
        public MainViewModel()
        {
            ImportData = new RelayCommand(ImportFile, CanImportFile);

        }
        private bool CanImportFile(object obj)
        {
            return true;
        }
        private void ImportFile(object obj)
        {
            string filePath = "";
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Excel | *.xlsx | Excel 2003 | *.xls";
            if (dialog.ShowDialog() == true)
            {
                filePath = dialog.FileName;
            }
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Invalid!");
            }
            var package = new ExcelPackage(new FileInfo(filePath));
            foreach(var workSheet in package.Workbook.Worksheets)
            {
                DataTable dt = new DataTable();
                bool hasHeader = true;
                int startRow = hasHeader ? 2 : 1;

                foreach (var cell in workSheet.Cells[1, 1, 1, workSheet.Dimension.End.Column])
                {
                    dt.Columns.Add(hasHeader ? cell.Text : $"Column {cell.Start.Column}");
                }

                for (int rowNum = startRow; rowNum <= workSheet.Dimension.End.Row; rowNum++)
                {
                    var row = dt.NewRow();
                    int colIndex = 0;
                    foreach (var cell in workSheet.Cells[rowNum, 1, rowNum, workSheet.Dimension.End.Column])
                    {
                        row[colIndex++] = cell.Text;
                    }
                    dt.Rows.Add(row);
                }
            }
        }
        public ICommand ExportData { get; set; }

    }
}
