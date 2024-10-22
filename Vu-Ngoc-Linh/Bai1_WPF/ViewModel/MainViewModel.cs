using System;
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
<<<<<<< HEAD
        public event PropertyChangedEventHandler PropertyChanged;
=======
        public MainModel Model { get; private set; }

        public string FilePath_2
        {
            get => Model.FilePath;
            set
            {
                Model.FilePath = value;
                OnPropertyChanged();
            }
        }

>>>>>>> e5d2876c7c9a761b7ce1717befce8c2a9bf03d93
        public ICommand ImportData { get; set; }
        public ICommand ExportData { get; set; }

        private DataTable _data;
        private string _filePath;
        private ObservableCollection<string> _sheetNames;
<<<<<<< HEAD
        private string _selectedSheet;
        private ObservableCollection<DataTable> Tables { get; set; }
        public MainModel Model { get; private set; }
=======

        public ObservableCollection<string> SheetNames
        {
            get { return _sheetNames; }
            set
            {
                _sheetNames = value;
                OnPropertyChanged(nameof(SheetNames));
            }
        }

        private string _selectedSheet;

        public string SelectedSheet
        {
            get { return _selectedSheet; }
            set
            {
                //if (_selectedSheet != value)
                {
                    //if (Data != null) Data.Clear();
                    _selectedSheet = value;
                    OnPropertyChanged(nameof(SelectedSheet));
                    Upload(_selectedSheet);
                }
            }
        }

        public ObservableCollection<DataTable> Tables { get; set; }
        public DataTable SelectedTable { get; set; }

        public Dictionary<string, DataTable> tables = new Dictionary<string, DataTable>();

        private void Upload(string selectedSheet)
        {
            foreach (string sheetname in tables.Keys)
            {
                if (sheetname == selectedSheet)
                {
                    Data = tables.GetValueOrDefault(sheetname);
                }
            }
        }

>>>>>>> e5d2876c7c9a761b7ce1717befce8c2a9bf03d93
        public MainViewModel()
        {
            _sheetNames = new ObservableCollection<string>();
            Tables = new ObservableCollection<DataTable>();
            ImportData = new RelayCommand(ImportFile, CanImportFile);
            ExportData = new RelayCommand(ExportFile, CanExportFile);
        }

<<<<<<< HEAD
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
=======
        public event PropertyChangedEventHandler PropertyChanged;

        private DataTable _data;
>>>>>>> e5d2876c7c9a761b7ce1717befce8c2a9bf03d93

        public DataTable Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }

<<<<<<< HEAD
=======
        private string _filePath;

>>>>>>> e5d2876c7c9a761b7ce1717befce8c2a9bf03d93
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> SheetNames
        {
            get { return _sheetNames; }
            set
            {
                _sheetNames = value;
                OnPropertyChanged(nameof(SheetNames));
            }
        }

        public string SelectedSheet
        {
            get { return _selectedSheet; }
            set
            {
                _selectedSheet = value;
                OnPropertyChanged(nameof(SelectedSheet));
                Upload(_selectedSheet);
            }
        }

        private void Upload(string selectedSheet)
        {
            foreach (DataTable dt in Tables)
            {
                if (dt.TableName == selectedSheet)
                {
                    Data = dt;
                    break;
                }
            }
        }

        private bool CanImportFile(object obj)
        {
            return true;
        }

<<<<<<< HEAD
=======
        //public DataTable Student, Teacher, Employee;

>>>>>>> e5d2876c7c9a761b7ce1717befce8c2a9bf03d93
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
                SheetNames.Clear();
<<<<<<< HEAD
                Tables.Clear(); 
=======
>>>>>>> e5d2876c7c9a761b7ce1717befce8c2a9bf03d93
                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    DataTable dt = new DataTable();
                    //dt.TableName = worksheet.Name; ??
                    dt = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column,
                                    worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].ToDataTable();
                    dt.TableName = worksheet.Name;
                    Tables.Add(dt);
                    SheetNames.Add(dt.TableName);
                }
                SelectedSheet = SheetNames.FirstOrDefault();
<<<<<<< HEAD
=======

>>>>>>> e5d2876c7c9a761b7ce1717befce8c2a9bf03d93
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
                    foreach (DataTable dt in Tables)
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
<<<<<<< HEAD
=======

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
>>>>>>> e5d2876c7c9a761b7ce1717befce8c2a9bf03d93
    }
}