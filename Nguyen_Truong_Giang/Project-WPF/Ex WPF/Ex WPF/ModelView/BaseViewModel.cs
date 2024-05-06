using ExcelDataReader;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace Ex_WPF.ModelView
{
    internal class BaseViewModel : MainViewModel
    {
        public ObservableCollection<Person> _selection = new ObservableCollection<Person>();

        private Brush _mouseEnter = new SolidColorBrush(Colors.LightCyan);

        private Brush _mouseLeave = new SolidColorBrush(Colors.White);

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

        private ObservableCollection<DataTable> _dataTables;

        public ObservableCollection<DataTable> DataTables
        {
            get => _dataTables;
            set
            {
                _dataTables = value;
                RaisePropertiesChanged(nameof(DataTables));
            }
        }

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

        public ICommand ImportFileCommand { get; set; }
        public ICommand ExportFileCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        public BaseViewModel()
        {
            InitData();
            InitCommands();
        }

        #region Init

        private void InitData()
        {
            _dataTables = new ObservableCollection<DataTable>();
            _dataTable = _dataTables.FirstOrDefault();
        }

        private void InitCommands()
        {
            ImportFileCommand = new RelayCommand(ImportFile);
            ExportFileCommand = new RelayCommand<object>(ExportFile);
            ClearCommand = new RelayCommand<object>(Clear);
        }

        #endregion Init

        #region Command implementations

        public void ImportFile()
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
                        // Đọc dữ liệu từ sheet vào DataTable
                        reader.Read();

                        var sheets = reader.AsDataSet(new ExcelDataSetConfiguration()
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
                        reader.Close();
                    }
                    stream.Close();
                }
            }
        }

        public void ExportFile(object obj)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx;*.xls;*.xlsm";

            // Lặp qua từng sheet trong danh sách sheets
            if (saveFileDialog.ShowDialog() == true)
            {
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    foreach (DataTable sheet in _dataTables)
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

        #endregion Command implementations
    }
}