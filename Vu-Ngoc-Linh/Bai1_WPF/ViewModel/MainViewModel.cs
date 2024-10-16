
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
        public ICommand ImportData { get; set; }
        public ICommand ExportData { get; set; }

        private ObservableCollection<string> _sheetNames;
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
                if (_selectedSheet != value)
                {
                    //if (Data != null) Data.Clear();
                    _selectedSheet = value;
                    OnPropertyChanged(nameof(SelectedSheet));
                    Upload(_selectedSheet);
                }
            }
        }

        private void Upload(string selectedSheet)
        {
            if (selectedSheet == "Student")
            {
                Data = Student;
            }
            else if (selectedSheet == "Teacher")
            {
                Data = Teacher;
            }
            else if (selectedSheet == "Employee")
            {
                Data = Employee;
            }
            else
            {
                Data = new DataTable(); 
            }
        }


        public MainViewModel()
        {
            _sheetNames = new ObservableCollection<string>();
            ImportData = new RelayCommand(ImportFile, CanImportFile);
            ExportData = new RelayCommand(ExportFile, CanExportFile);

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private DataTable _data;
        public DataTable Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }
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

        private bool CanImportFile(object obj)
        {
            return true;
        }
        public DataTable Student, Teacher, Employee;
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
                SheetNames.Clear();
                foreach ( ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    SheetNames.Add(worksheet.Name);
                    dt = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column,
                                    worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].ToDataTable();
                    if(worksheet.Name == "Student")
                    {
                        Student = dt;
                    }
                    else if(worksheet.Name == "Teacher")
                    {
                        Teacher = dt;
                    }
                    else
                    {
                        Employee = dt;
                    }
                }
                if (SheetNames.Any())
                {
                    SelectedSheet = SheetNames.First();
                }

            }
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
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
