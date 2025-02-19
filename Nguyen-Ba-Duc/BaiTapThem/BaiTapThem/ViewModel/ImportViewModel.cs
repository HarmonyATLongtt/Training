using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaiTapThem.Model;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using OfficeOpenXml;
using System.IO;
using System.Windows;
using System.Data;

namespace BaiTapThem.ViewModel
{
    public class ImportViewModel : BaseViewModel
    {
        private bool _isEnable = false;

        public bool IsEnable
        {
            get { return _isEnable; }
            set { _isEnable = value; OnPropertyChanged(); }
        }

        private Visibility _cmbIsVisible = Visibility.Hidden;

        public Visibility CmbIsVisible
        {
            get { return _cmbIsVisible; }
            set { _cmbIsVisible = value; OnPropertyChanged(nameof(CmbIsVisible)); }
        }

        private ObservableCollection<string> _comboboxItems;

        public ObservableCollection<string> ComboboxItems
        {
            get { return _comboboxItems; }
            set
            {
                _comboboxItems = value;

                OnPropertyChanged();
            }
        }

        private int _selectedIndex;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                DataGridCurrent = DataTables[SelectedIndex];
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ExcelCellMappingModel> _excelMapping;

        public ObservableCollection<ExcelCellMappingModel> ExcelCellMappings
        {
            get { return _excelMapping; }
            set
            {
                _excelMapping = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DataTable> _dataTables;

        public ObservableCollection<DataTable> DataTables
        {
            get { return _dataTables; }
            set
            {
                _dataTables = value;
                OnPropertyChanged();
            }
        }

        private DataTable _dataGridCurrent;

        public DataTable DataGridCurrent
        {
            get { return _dataGridCurrent; }
            set
            {
                _dataGridCurrent = value;
                OnPropertyChanged();
            }
        }

        private string filePath = "";

        public RelayCommand ImportCommand { get; set; }
        public RelayCommand ExportCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }

        public ImportViewModel()
        {
            //ExcelMappingList = new ObservableCollection<ObservableCollection<ExcelCellMappingModel>>();
            ExcelCellMappings = new ObservableCollection<ExcelCellMappingModel>();
            ComboboxItems = new ObservableCollection<string>();
            DataTables = new ObservableCollection<DataTable>();
            DataGridCurrent = new DataTable();

            //SelectedIndex = 0;

            ImportCommand = new RelayCommand(ImportFile);
            ExportCommand = new RelayCommand(ExportFile);
            SaveCommand = new RelayCommand(SaveFile);
        }

        private void ImportFile(object p)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("The path is invalid");
            }
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            var package = new ExcelPackage(new FileInfo(filePath));

            List<String> listSheetName = new List<String>();

            List<DataTable> dataTables = new List<DataTable>();
            var worksheets = package.Workbook.Worksheets;
            List<ExcelCellMappingModel> listExcelCellMapping = new List<ExcelCellMappingModel>();
            //foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
            for (int a = 0; a < worksheets.Count; a++)
            {
                var worksheet = worksheets[a];
                listSheetName.Add(worksheet.Name);

                var totalRows = worksheet.Dimension.End.Row;

                var totalColumns = worksheet.Dimension.End.Column;

                DataTable dt = new DataTable();

                foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                {
                    dt.Columns.Add(firstRowCell.Text);
                }
                for (int i = 2; i <= totalRows; i++)
                {
                    var newRow = dt.NewRow();
                    for (var j = 1; j <= totalColumns; j++)
                    {
                        newRow[j - 1] = worksheet.Cells[i, j].Value;
                        //Type type = worksheet.Cells[i, j].GetType();
                        var excelMapping = new ExcelCellMappingModel { SheetIndex = a, ExcelRow = i, ExcelColumn = j, UiRow = i - 2, UiColumn = j - 1, IsChanged = false, Value = worksheet.Cells[i, j].Value.ToString() };
                        listExcelCellMapping.Add(excelMapping);
                    }
                    dt.Rows.Add(newRow);
                }
                dataTables.Add(dt);
            }
            ExcelCellMappings = new ObservableCollection<ExcelCellMappingModel>(listExcelCellMapping);
            DataTables = new ObservableCollection<DataTable>(dataTables);
            //DataGridCurrent = DataTables[0];
            SelectedIndex = 0;
            ComboboxItems = new ObservableCollection<string>(listSheetName);
            IsEnable = true;
            CmbIsVisible = Visibility.Visible;
        }

        private void ExportFile(object p)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                filePath = saveFileDialog.FileName;
            }
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("The path is invalid");
                return;
            }
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                for (int i = 0; i < DataTables.Count; i++)
                {
                    var worksheet = package.Workbook.Worksheets.Add(ComboboxItems[i]);

                    worksheet.Cells.LoadFromDataTable(DataTables[i], true);

                    var totalRows = worksheet.Dimension.End.Row;

                    var totalColumns = worksheet.Dimension.End.Column;

                    for (int r = 1; r <= totalRows; r++)
                    {
                        for (int c = 1; c <= totalColumns; c++)
                        {
                            var valueText = worksheet.Cells[r, c].Text;
                            if (double.TryParse(valueText, out double numericValue))
                            {
                                worksheet.Cells[r, c].Value = numericValue;
                            }
                        }
                    }
                }
                package.SaveAs(new FileInfo(filePath));
            }
            MessageBox.Show("Xuất excel thành công!");
        }

        private void SaveFile(object p)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheets = package.Workbook.Worksheets;
                //for (int i = 0; i < DataTables.Count; i++)
                //{
                //    worksheets[i].Cells.LoadFromDataTable(DataTables[i], true);
                //}
                for (int i = 0; i < DataTables.Count; i++)
                {
                    var dataTable = DataTables[i];

                    for (int col = 0; col < dataTable.Columns.Count; col++)
                    {
                        worksheets[i].Cells[1, col + 1].Value = dataTable.Columns[col].ColumnName;
                        worksheets[i].Cells[1, col + 1].Style.Font.Bold = true;
                    }
                    for (int row = 0; row < dataTable.Rows.Count; row++)
                    {
                        for (int col = 0; col < dataTable.Columns.Count; col++)
                        {
                            var value = dataTable.Rows[row][col];
                            worksheets[i].Cells[row + 2, col + 1].Value = value;
                            var valueText = worksheets[i].Cells[row + 2, col + 1].Text;

                            if (double.TryParse(valueText, out double numericValue))
                            {
                                worksheets[i].Cells[row + 2, col + 1].Value = numericValue;
                            }
                        }
                    }
                }

                foreach (var excelCell in ExcelCellMappings)
                {
                    if (excelCell.IsChanged == true)
                    {
                        //var num = excelCell.SheetIndex;
                        worksheets[excelCell.SheetIndex].Cells[excelCell.ExcelRow, excelCell.ExcelColumn].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        worksheets[excelCell.SheetIndex].Cells[excelCell.ExcelRow, excelCell.ExcelColumn].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    }
                }

                package.Save();
            }
            MessageBox.Show("Lưu thành công");
        }
    }
}