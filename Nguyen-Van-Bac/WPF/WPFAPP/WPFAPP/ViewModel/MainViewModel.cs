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
            // Hiển thị hộp thoại SaveFileDialog để người dùng chọn vị trí và tên file Excel mới
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

                        // Khởi tạo dòng header cho mỗi sheet
                        worksheetStudent.Cells[1, 1].Value = "ID";
                        worksheetStudent.Cells[1, 2].Value = "Name";
                        worksheetStudent.Cells[1, 3].Value = "Age";
                        worksheetStudent.Cells[1, 4].Value = "Address";
                        worksheetStudent.Cells[1, 5].Value = "TaxCode";
                        worksheetStudent.Cells[1, 6].Value = "Income";
                        worksheetStudent.Cells[1, 7].Value = "School";
                        worksheetStudent.Cells[1, 8].Value = "Class";

                        worksheetTeacher.Cells[1, 1].Value = "ID";
                        worksheetTeacher.Cells[1, 2].Value = "Name";
                        worksheetTeacher.Cells[1, 3].Value = "Age";
                        worksheetTeacher.Cells[1, 4].Value = "Address";
                        worksheetTeacher.Cells[1, 5].Value = "TaxCode";
                        worksheetTeacher.Cells[1, 6].Value = "Income";
                        worksheetTeacher.Cells[1, 7].Value = "School";

                        worksheetEmployee.Cells[1, 1].Value = "ID";
                        worksheetEmployee.Cells[1, 2].Value = "Name";
                        worksheetEmployee.Cells[1, 3].Value = "Age";
                        worksheetEmployee.Cells[1, 4].Value = "Address";
                        worksheetEmployee.Cells[1, 5].Value = "TaxCode";
                        worksheetEmployee.Cells[1, 6].Value = "Income";
                        if (DataExportStudent != null)
                        {
                            for (int i = 0; i < DataExportStudent.Count; i++)
                            {
                                if (DataExportStudent[i] is Student student)
                                {
                                    int rowIndex = i + 2;
                                    worksheetStudent.Cells[rowIndex, 1].Value = student.ID;
                                    worksheetStudent.Cells[rowIndex, 2].Value = student.Name;
                                    worksheetStudent.Cells[rowIndex, 3].Value = student.Age;
                                    worksheetStudent.Cells[rowIndex, 4].Value = student.Address;
                                    worksheetStudent.Cells[rowIndex, 5].Value = student.TaxCode;
                                    worksheetStudent.Cells[rowIndex, 6].Value = student.Income;
                                    worksheetStudent.Cells[rowIndex, 7].Value = student.School;
                                    worksheetStudent.Cells[rowIndex, 8].Value = student.Class;
                                }
                            }
                        }
                        if (DataExportTeacher != null)
                        {
                            for (int i = 0; i < DataExportTeacher.Count; i++)
                            {
                                if (DataExportTeacher[i] is Teacher teacher)
                                {
                                    int rowIndex = i + 2;
                                    worksheetTeacher.Cells[rowIndex, 1].Value = teacher.ID;
                                    worksheetTeacher.Cells[rowIndex, 2].Value = teacher.Name;
                                    worksheetTeacher.Cells[rowIndex, 3].Value = teacher.Age;
                                    worksheetTeacher.Cells[rowIndex, 4].Value = teacher.Address;
                                    worksheetTeacher.Cells[rowIndex, 5].Value = teacher.TaxCode;
                                    worksheetTeacher.Cells[rowIndex, 6].Value = teacher.Income;
                                    worksheetTeacher.Cells[rowIndex, 7].Value = teacher.School;
                                }
                            }
                        }
                        if (DataExportEmployee != null)
                        {
                            for (int i = 0; i < DataExportEmployee.Count; i++)
                            {
                                if (DataExportEmployee[i] is Employee employee)
                                {
                                    int rowIndex = i + 2;
                                    worksheetEmployee.Cells[rowIndex, 1].Value = employee.ID;
                                    worksheetEmployee.Cells[rowIndex, 2].Value = employee.Name;
                                    worksheetEmployee.Cells[rowIndex, 3].Value = employee.Age;
                                    worksheetEmployee.Cells[rowIndex, 4].Value = employee.Address;
                                    worksheetEmployee.Cells[rowIndex, 5].Value = employee.TaxCode;
                                    worksheetEmployee.Cells[rowIndex, 6].Value = employee.Income;
                                }
                            }
                        }


                        // Lưu file Excel
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
    }
}