using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WPFAPP.Extension.Excel;
using WPFAPP.Model;

namespace WPFAPP.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            ImportCommand = new RelayCommand(ImportInvoke);
            SelectedSheetCommand = new RelayCommand<string>(SwitchSheet);
            ExportCommand = new RelayCommand(ExportInvoke);
            ImportExcelCommand = new RelayCommand(ImportExcel);
            SaveCommand = new RelayCommand(SaveInvoke);
        }

        private string _selectedSheet;

        public string SelectedSheet
        {
            get { return _selectedSheet; }
            set
            {
                if (_selectedSheet != value)
                {
                    _selectedSheet = value;
                    NotifyChanged("SelectedSheet");
                    SelectedSheetCommand.Execute(null); // Gọi SelectedSheetCommand khi selected sheet thay đổi
                }
            }
        }

        private ObservableCollection<string> _sheetNames;

        public ObservableCollection<string> SheetNames
        {
            get { return _sheetNames; }
            set { _sheetNames = value; NotifyChanged("SheetNames"); }
        }

        private string _selectedSheetName;

        public string SelectedSheetName
        {
            get { return _selectedSheetName; }
            set
            {
                _selectedSheetName = value;
                LoadSelectedSheetData();
            }
        }

        private ObservableCollection<ObservableCollection<string>> _selectedSheetData;

        public ObservableCollection<ObservableCollection<string>> SelectedSheetData
        {
            get { return _selectedSheetData; }
            set { _selectedSheetData = value; NotifyChanged("SelectedSheetData"); }
        }

        public ICommand ImportExcelCommand { get; }
        public ObservableCollection<object> Data { get; set; }
        public ObservableCollection<object> DataExportStudent { get; set; }
        public ObservableCollection<object> DataExportTeacher { get; set; }
        public ObservableCollection<object> DataExportEmployee { get; set; }
        public ICommand SaveCommand { get; } 
        // Command to handle the selection of a new sheet
        public ICommand SelectedSheetCommand { get; }

        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ExcelFileInfo FileInfo { get; set; }
        public string FilePath { get; set; }

        private void ImportInvoke(object sender)
        {
            ChooseFileInvoke();
            if (!string.IsNullOrEmpty(FilePath))
            {
                var analyzer = new ExcelAnalyzer();
                FileInfo = analyzer.AnalyzerFile(FilePath);
                ObservableCollection<string> sheetNam = new ObservableCollection<string>(FileInfo.SheetNames);
                SheetNames = sheetNam;
                NotifyChanged("SheetNames");
            }
        }

        private void ExportInvoke(object sender)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    using (var package = new ExcelPackage())
                    {
                        ExcelWorksheet worksheetStudent = package.Workbook.Worksheets.Add("Student");
                        ExcelWorksheet worksheetTeacher = package.Workbook.Worksheets.Add("Teacher");
                        ExcelWorksheet worksheetEmployee = package.Workbook.Worksheets.Add("Employee");

                        string[] studentHeaders = { "ID", "Name", "Age", "Address", "TaxCode", "Income", "School", "Class" };
                        string[] teacherHeaders = { "ID", "Name", "Age", "Address", "TaxCode", "Income", "School" };
                        string[] employeeHeaders = { "ID", "Name", "Age", "Address", "TaxCode", "Income" };

                        SetHeader(worksheetStudent, studentHeaders);
                        SetHeader(worksheetTeacher, teacherHeaders);
                        SetHeader(worksheetEmployee, employeeHeaders);

                        ExportDataToWorksheet(DataExportStudent, worksheetStudent);
                        ExportDataToWorksheet(DataExportTeacher, worksheetTeacher);
                        ExportDataToWorksheet(DataExportEmployee, worksheetEmployee);

                        package.SaveAs(new FileInfo(filePath));
                    }

                    MessageBox.Show("Dữ liệu đã được xuất thành công vào " + filePath, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Xuất dữ liệu không thành công. Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        private void ChooseFileInvoke()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
            }
        }

        private void SwitchSheet(string selectedSheet)
        {
            switch (SelectedSheet)
            {
                case "Student":
                    Data = ExcelReader.ReadStudents(FilePath);
                    DataExportStudent = Data;
                    NotifyChanged("Data");
                    break;

                case "Teacher":
                    Data = ExcelReader.ReadTeachers(FilePath);
                    DataExportTeacher = Data;
                    NotifyChanged("Data");
                    break;

                case "Employees":
                    Data = ExcelReader.ReadEmployees(FilePath);
                    DataExportEmployee = Data;
                    NotifyChanged("Data");
                    break;

                default:
                    break;
            }
        }

        private void LoadSelectedSheetData()
        {
            if (!string.IsNullOrEmpty(SelectedSheetName))
            {
                SelectedSheetData = ExcelReader.ReadExcel(FilePath, SelectedSheetName); // Thay thế YourExcelReader bằng lớp bạn sử dụng để đọc dữ liệu từ file Excel
                NotifyChanged("SelectedSheetData");
                NotifyChanged("Data");
            }
        }
        private void SaveInvoke(object obj)
        {
            switch (SelectedSheet)
            {
                case "Student":
                    DataExportStudent = Data;
                    break;

                case "Teacher":
                    DataExportTeacher = Data;
                    break;

                case "Employees":
                    DataExportEmployee = Data;
                    break;

                default:
                    break;
            }
        }

        private void ImportExcel(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
            if (openFileDialog.ShowDialog() == true)
            {
                FilePath = openFileDialog.FileName;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage package = new ExcelPackage(new System.IO.FileInfo(openFileDialog.FileName)))
                {
                    SheetNames = new ObservableCollection<string>();
                    foreach (var worksheet in package.Workbook.Worksheets)
                    {
                        SheetNames.Add(worksheet.Name);
                    }
                }
            }
        }

        private void SetHeader(ExcelWorksheet worksheet, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }
        }

        private void ExportDataToWorksheet(ObservableCollection<object> dataList, ExcelWorksheet worksheet)
        {
            if (dataList != null)
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    if (dataList[i] is Student student)
                    {
                        int rowIndex = i + 2;
                        worksheet.Cells[rowIndex, 1].Value = student.ID;    
                        worksheet.Cells[rowIndex, 2].Value = student.Name;
                        worksheet.Cells[rowIndex, 3].Value = student.Age;
                        worksheet.Cells[rowIndex, 4].Value = student.Address;
                        worksheet.Cells[rowIndex, 5].Value = student.TaxCode;
                        worksheet.Cells[rowIndex, 6].Value = student.Income;
                        worksheet.Cells[rowIndex, 7].Value = student.School;
                        worksheet.Cells[rowIndex, 8].Value = student.Class;
                    }
                    else if (dataList[i] is Teacher teacher)
                    {
                        int rowIndex = i + 2;
                        worksheet.Cells[rowIndex, 1].Value = teacher.ID;
                        worksheet.Cells[rowIndex, 2].Value = teacher.Name;
                        worksheet.Cells[rowIndex, 3].Value = teacher.Age;
                        worksheet.Cells[rowIndex, 4].Value = teacher.Address;
                        worksheet.Cells[rowIndex, 5].Value = teacher.TaxCode;
                        worksheet.Cells[rowIndex, 6].Value = teacher.Income;
                        worksheet.Cells[rowIndex, 7].Value = teacher.School;
                    }
                    else if (dataList[i] is Employee employee)
                    {
                        int rowIndex = i + 2;
                        worksheet.Cells[rowIndex, 1].Value = employee.ID;
                        worksheet.Cells[rowIndex, 2].Value = employee.Name;
                        worksheet.Cells[rowIndex, 3].Value = employee.Age;
                        worksheet.Cells[rowIndex, 4].Value = employee.Address;
                        worksheet.Cells[rowIndex, 5].Value = employee.TaxCode;
                        worksheet.Cells[rowIndex, 6].Value = employee.Income;
                    }
                }
            }
        }
    }
}