using ExcelDataReader;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace Ex_WPF.ModelView
{
    class BaseViewModel : MainViewModel
    {
        public ObservableCollection<Person> _selection = new ObservableCollection<Person>();

        private Brush _mouseEnter = new SolidColorBrush(Colors.LightCyan);

        private Brush _mouseLeave = new SolidColorBrush(Colors.White);

        public string sheetName;
        public string SheetName
        {
            get => sheetName;
            set
            {
                sheetName = value;
                RaisePropertiesChanged(nameof(SheetName));
            }
        }


        public Brush MouseHover
        {
            get { return _mouseEnter; }
            set
            {
                _mouseEnter = value;
                RaisePropertiesChanged("MouseHover");
            }
        }
        public Brush MouseLeave
        {
            get { return _mouseLeave; }
            set
            {
                _mouseLeave = value;
                RaisePropertiesChanged("MouseLeave");
            }
        }
        public ObservableCollection<Person> Selection
        {
            get => _selection;
            set
            {
                _selection = value;
                RaisePropertiesChanged(nameof(Selection));
            }
        }
        private List<DataTable> _dataTables = new List<DataTable>();
        private DataTable _dataTable;
        public DataTable DataTable
        {
            get => _dataTable;
            set
            {
                _dataTable = value;
                RaisePropertiesChanged(nameof(DataTable));
            }
        }

        //public ObservableCollection<Person> _student = new ObservableCollection<Person>();

        //public ObservableCollection<Person> _teacher = new ObservableCollection<Person>();

        //public ObservableCollection<Person> _employee = new ObservableCollection<Person>();

        //public ObservableCollection<Person> _nextSheet = new ObservableCollection<Person>();

        //public ObservableCollection<Person> _backSheet = new ObservableCollection<Person>();


        public ICommand ImportFileCommand { get; set; }
        public ICommand ExportFileCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand NextSheetCommand { get; set; }
        public ICommand BackSheetCommand { get; set; }

        public BaseViewModel()
        {
            ImportFileCommand = new RelayCommand<object>(ImportFile);
            ExportFileCommand = new RelayCommand<object>(ExportFile);
            ClearCommand = new RelayCommand<object>(Clear);
            NextSheetCommand = new RelayCommand<object>(NextSheet);
            BackSheetCommand = new RelayCommand<object>(BackSheet);
        }

        private int index = 0;
        private DataTableCollection sheets;

        private void InitSheet(int indexSheet)
        {
            if (index <= sheets.Count - 1)
            {
                _selection.Clear();

                var bindingList = new BindingList<Person>();
                DataTable sheet = sheets[index];

                var sheetNames = sheet.TableName;
                SheetName = sheetNames;

            }
        }

        public void ImportFile(object obj)
        {
            // Khởi tạo OpenFileDialog để lựa chọn file excel
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

            if (openFileDialog.ShowDialog() == true)
            {
                // Lấy đường dẫn của file excel đã chọn
                string filePath = openFileDialog.FileName;

                // Khởi tạo FileStream để đọc file excel
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet();
                        var sheetNames = dataSet.Tables.Cast<DataTable>().Select(table => table.TableName).ToList();
                        // Đọc dữ liệu từ sheet vào DataTable
                        reader.Read();

                        sheets = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        }).Tables;
                        _dataTables.Clear();
                        foreach (DataTable sheet in sheets)
                        {
                            _dataTables.Add(sheet);
                        }

                        DataTable = _dataTables.FirstOrDefault();
                        SheetName = DataTable.TableName;
                        // InitSheet(0);
                        reader.Close();
                    }
                    stream.Close();
                }

                //  Selection = _selection;
            }
        }

        public void ExportFile(object obj)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx;*.xls;*.xlsm";


            //// Tạo package mới
            //ExcelPackage excelPackage = new ExcelPackage();

            //// Đặt tên file
            //string fileName = "export.xlsx";

            // Lặp qua từng sheet trong danh sách sheets
            if (saveFileDialog.ShowDialog() == true)
            {
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    foreach (DataTable sheet in sheets)
                    {  // Tạo một worksheet mới trong package
                        ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add(sheet.TableName);
                        for (int j = 0; j < sheet.Columns.Count; j++)
                        {
                            excelWorksheet.Cells[1, j + 1].Value = sheet.Columns[j].ColumnName;
                        }

                        // Đổ dữ liệu vào worksheet
                        for (int i = 0; i < sheet.Rows.Count; i++)
                        {
                            for (int j = 0; j < sheet.Columns.Count; j++)
                            {
                                excelWorksheet.Cells[i + 2, j + 1].Value = sheet.Rows[i][j];
                            }
                        }
                    }
                    excelPackage.SaveAs(new FileStream(saveFileDialog.FileName, FileMode.Create));
                    excelPackage.Dispose();
                    MessageBox.Show("Export successful");
                }
            }
        }

        public void Clear(object obj)
        {
            _selection.Clear();
            _dataTable.Clear();
            _dataTables.Clear();
            MessageBox.Show("Clear successful!");
        }

        public void NextSheet(object obj)
        {
            index++;
            if (_dataTables.IndexOf(DataTable) >= 0)
            {
                InitSheet(index);
                int i = _dataTables.IndexOf(DataTable);
                DataTable = _dataTables[(i + 1) % _dataTables.Count];
            }
            if (index >= _dataTables.Count)
            {
                index = 0;
                InitSheet(index);
                new DataTable();
            }

            //SheetName = DataTable.TableName;
            //DataTable = i >= 0 ? _dataTables[(i + 1) % _dataTables.Count] : new DataTable();
        }

        public void BackSheet(object obj)
        {


            if (_dataTables.Count >= index)
            {
                InitSheet(index--);
                int i = _dataTables.IndexOf(DataTable);
                DataTable = _dataTables[(i - 1) % _dataTables.Count];
            }

            //int i = _dataTables.IndexOf(DataTable);
            //SheetName = DataTable.TableName;
            //DataTable = i >= 0 ? _dataTables[(i - 1) % _dataTables.Count] : new DataTable();
        }
    }
}
