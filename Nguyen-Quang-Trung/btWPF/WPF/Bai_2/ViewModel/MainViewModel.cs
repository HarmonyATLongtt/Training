using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Bai_2.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public DataTable _currentSheet;

        public DataTable CurrentSheet
        {
            get { return _currentSheet; }
            set
            {
                _currentSheet = value;
                OnPropertyChanged(nameof(CurrentSheet));
            }
        }

        public List<string> _sheetName;

        public List<string> SheetName
        {
            get { return _sheetName; }
            set
            {
                _sheetName = value;
                OnPropertyChanged(nameof(SheetName));
            }
        }

        private List<DataTable> _listDataTable = new List<DataTable>();
        public List<string> _listSheet = new List<string>();

        //private DataTable _selectedSheetData;

        //public DataTable SelectedSheetData
        //{
        //    get { return _selectedSheetData; }
        //    set
        //    {
        //        _selectedSheetData = value;
        //        OnPropertyChanged(nameof(SelectedSheetData));
        //    }
        //}

        public ICommand ImportFileCommand { get; set; }
        public ICommand ExportFileCommand { get; set; }
        public ICommand SelectSheetCommand { get; set; }

        public MainViewModel()
        {
            ImportFileCommand = new RelayCommand<object>(ImportFile);
            ExportFileCommand = new RelayCommand<object>(ExportFile);
            SelectSheetCommand = new RelayCommand<object>(SelectSheet);
        }

        public void ExportFile(object obj)
        {
            string filePath = "";
            SaveFileDialog exportFile = new SaveFileDialog();
            exportFile.Filter = "Excel files (*.xlsx)|*.xlsx|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            exportFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (exportFile.ShowDialog() == true)
            {
                filePath = exportFile.FileName;
            }
            else if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Đường dẫn không hợp lệ");
                return;
            }
            var package = new ExcelPackage(new FileInfo(filePath));
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            foreach (var dataTable in _listDataTable)
            {
                if (dataTable != null && !string.IsNullOrEmpty(dataTable.TableName))
                {
                    var workSheet = package.Workbook.Worksheets.Add(dataTable.TableName);
                    workSheet.Cells.LoadFromDataTable(dataTable, true);
                }
            }
            package.Save();
        }

        public void SelectSheet(object obj)
        {
            if (SheetName == null || SheetName.Count == 0)
            {
                return;
            }
            string selectedSheet = obj as string;
            int index = SheetName.IndexOf(selectedSheet);
            CurrentSheet = _listDataTable[index];
            OnPropertyChanged(nameof(CurrentSheet));
        }

        public void ImportFile(object obj)
        {
            string filePath = "";
            //var listDataTable = new List<DataTable>();

            //var listSheet = new List<string>();

            var importFileDialog = new OpenFileDialog();
            importFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            importFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (importFileDialog.ShowDialog() == true)
            {
                filePath = importFileDialog.FileName;
            }
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Đường dẫn không hợp lệ");
                return;
            }
            var package = new ExcelPackage(new FileInfo(filePath));
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            int i = 1;
            foreach (var workSheet in package.Workbook.Worksheets)
            {
                var dataTable = new DataTable();
                dataTable.TableName = $"Table ({i})";
                dataTable = workSheet.Cells[1, 1, workSheet.Dimension.End.Row, workSheet.Dimension.End.Column].ToDataTable(c =>
                {
                    c.FirstRowIsColumnNames = true;
                });
                string newTableName = workSheet.Name;
                while (_listDataTable.Any(dt => dt.TableName == newTableName))
                {
                    newTableName = $"{workSheet.Name} ({i})";
                    i++;
                }

                // Gán tên mới cho bảng
                dataTable.TableName = newTableName;

                _listDataTable.Add(dataTable);
                _listSheet.Add(workSheet.Name);
            }
            SheetName = _listSheet;
            CurrentSheet = _listDataTable[0];
        }
    }
}