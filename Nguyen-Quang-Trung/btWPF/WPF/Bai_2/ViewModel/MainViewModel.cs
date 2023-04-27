using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<string> _sheetName;

        public ObservableCollection<string> SheetName
        {
            get { return _sheetName; }
            set
            {
                _sheetName = value;
                OnPropertyChanged(nameof(SheetName));
            }
        }

        private ObservableCollection<DataTable> _listDataTable = new ObservableCollection<DataTable>();
        public ObservableCollection<string> _listSheet = new ObservableCollection<string>();

        //private string _selectedSheet;

        //public string SelectedSheet
        //{
        //    get { return _selectedSheet; }
        //    set
        //    {
        //        _selectedSheet = value;
        //        int index = SheetName.IndexOf(value);
        //        CurrentSheet = _listDataTable[index];
        //    }
        //}

        public ICommand ImportFileCommand { get; set; }
        public ICommand ExportFileCommand { get; set; }
        //public ICommand SelectSheetCommand { get; set; }

        public MainViewModel()
        {
            ImportFileCommand = new RelayCommand<object>(ImportFile);
            ExportFileCommand = new RelayCommand<object>(ExportFile);
            //SelectSheetCommand = new RelayCommand<object>(SelectSheet);
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

        //public void SelectSheet(object obj)
        //{
        //    if (SheetName == null || SheetName.Count == 0)
        //    {
        //        return;
        //    }
        //    string selectedSheet = obj as string;
        //    int index = SheetName.IndexOf(selectedSheet);
        //    CurrentSheet = _listDataTable[index];
        //}

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
            int x = 1;
            foreach (var workSheet in package.Workbook.Worksheets)
            {
                var dataTable = new DataTable();
                // Thêm tên cột vào DataTable
                for (int i = 1; i <= workSheet.Dimension.End.Column; i++)
                {
                    dataTable.Columns.Add(workSheet.Cells[1, i].Value.ToString());
                }
                // Duyệt từng cell trong tệp Excel và thêm vào DataTable
                for (int row = 2; row <= workSheet.Dimension.End.Row; row++)
                {
                    DataRow dr = dataTable.NewRow();

                    for (int col = 1; col <= workSheet.Dimension.End.Column; col++)
                    {
                        if (workSheet.Cells[row, col].Value != null)
                        {
                            if (workSheet.Cells[row, col].Value.GetType() == typeof(DateTime))
                            {
                                dr[col - 1] = workSheet.Cells[row, col].GetValue<DateTime>();
                            }
                            else
                            {
                                dr[col - 1] = workSheet.Cells[row, col].Value.ToString();
                            }
                        }
                        else
                        {
                            dr[col - 1] = DBNull.Value;
                        }
                    }

                    dataTable.Rows.Add(dr);
                }

                string newTableName = workSheet.Name;
                while (_listDataTable.Any(dt => dt.TableName == newTableName))
                {
                    newTableName = $"{workSheet.Name} ({x})";
                    x++;
                }
                // Gán tên mới cho bảng
                dataTable.TableName = newTableName;

                _listDataTable.Add(dataTable);
                _listSheet.Add(workSheet.Name);
            }
            SheetName = _listSheet;
            CurrentSheet = _listDataTable[2];
        }
    }
}