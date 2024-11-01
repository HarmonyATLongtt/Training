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
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ImportData { get; set; }
        public ICommand ExportData { get; set; }

        private DataTable _data;
        private string _filePath;
        private ObservableCollection<string> _sheetNames;
        private string _selectedSheet;
        private ObservableCollection<DataTable> Tables { get; set; }
        public MainModel Model { get; private set; }
        public MainViewModel()
        {
            _sheetNames = new ObservableCollection<string>();
            Tables = new ObservableCollection<DataTable>();
            ImportData = new RelayCommand(ImportFile, CanImportFile);
            ExportData = new RelayCommand(ExportFile, CanExportFile);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                Tables.Clear(); 
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
                        ExcelWorksheet ws = pkg.Workbook.Worksheets.Add(dt.TableName); //them sheet
                        ws.Cells["A1"].LoadFromDataTable(dt, true); //load data tu dt vao sheet tu o A1, true la load ca header
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
