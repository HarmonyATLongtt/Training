using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Windows.Input;
using System.Windows;
using WPF_Ex.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using LicenseContext = OfficeOpenXml.LicenseContext;
using System.Runtime.CompilerServices;

namespace WPF_Ex
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public MainModel Model { get; private set; }

        public string FileName
        {
            get => Model.FileName;
            set
            {
                Model.FileName = value;
                OnPropertyChanged();
            }
        }

        public string SelectedSheetData
        {
            get => Model.SelectedSheetData;
            set
            {
                Model.SelectedSheetData = value;
                OnPropertyChanged();
                Display();
            }
        }

        public ObservableCollection<string> SheetNames
        {
            get => Model.SheetNames;
            set
            {
                Model.SheetNames = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Student> Students
        {
            get => Model.Students;
            set
            {
                Model.Students = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Teacher> Teachers
        {
            get => Model.Teachers;
            set
            {
                Model.Teachers = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Employee> Employees
        {
            get => Model.Employees;
            set
            {
                Model.Employees = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public MainViewModel()
        {
            Model = new MainModel();
            LoadCommand = new RelayCommand(LoadExcelFile, CanImportFile);
            ExportCommand = new RelayCommand(ExportToExcel, CanExportFile);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CanImportFile(object obj) => true;

        private void LoadExcelFile(object obj)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true) return;

            FileName = dialog.FileName;

            using (var package = new ExcelPackage(new FileInfo(FileName)))
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                SheetNames.Clear();
                Students.Clear();
                Teachers.Clear();
                Employees.Clear();

                // Duyệt qua tất cả các worksheet và xử lý trực tiếp
                foreach (var worksheet in package.Workbook.Worksheets)
                {
                    string sheetName = worksheet.Name;
                    SheetNames.Add(sheetName);

                    
                    switch (sheetName)
                    {
                        case "Student":
                            LoadStudentsFromSheet(worksheet);
                            break;

                        case "Teacher":
                            LoadTeachersFromSheet(worksheet);
                            break;

                        case "Employee":
                            LoadEmployeesFromSheet(worksheet);
                            break;

                        
                        default:
                            break;
                    }
                }

                SelectedSheetData = SheetNames.FirstOrDefault();
            }
        }



        private void LoadStudentsFromSheet(ExcelWorksheet worksheet)
        {
            var dim = worksheet.Dimension;
            if (dim == null) return;

            for (int row = worksheet.Dimension.Start.Row + 1; row <= worksheet.Dimension.End.Row; row++)
            {
                if (string.IsNullOrEmpty(worksheet.Cells[row, 1].Text)) // Ví dụ kiểm tra
                    continue;  // Đảm bảo continue nằm trong vòng lặp

                var student = new Student
                {
                    ID = worksheet.Cells[row, 1].Text,
                    Name = worksheet.Cells[row, 2].Text,
                    Age = int.TryParse(worksheet.Cells[row, 3].Text, out int age) ? age : 0,
                    Class = worksheet.Cells[row, 4].Text,
                    School = worksheet.Cells[row, 5].Text
                };
                Students.Add(student);
            }
        }


        private void LoadTeachersFromSheet(ExcelWorksheet worksheet)
        {
            var dim = worksheet.Dimension;
            if (dim == null) return;

            for (int row = worksheet.Dimension.Start.Row + 1; row <= worksheet.Dimension.End.Row; row++)
            {
                if (string.IsNullOrEmpty(worksheet.Cells[row, 1].Text)) // Ví dụ kiểm tra
                    continue;  // Đảm bảo continue nằm trong vòng lặp
                var teacher = new Teacher
                {
                    ID = worksheet.Cells[row, 1].Text, // Cột 1: ID
                    Name = worksheet.Cells[row, 2].Text, // Cột 2: Name
                    Age = int.TryParse(worksheet.Cells[row, 3].Text, out int age) ? age : 0, // Cột 3: Age
                    School = worksheet.Cells[row, 4].Text, // Cột 4: School
                    Income = double.TryParse(worksheet.Cells[row, 5].Text, out double income) ? income : 0.0, // Cột 5: Income
                    TaxCoe = double.TryParse(worksheet.Cells[row, 6].Text, out double taxCoe) ? taxCoe : 0.0 // Cột 6: TaxCoe
                };
                Teachers.Add(teacher); // Thêm dữ liệu vào ObservableCollection
            }
        }

        private void LoadEmployeesFromSheet(ExcelWorksheet worksheet)
        {
            var dim = worksheet.Dimension;
            if (dim == null) return;

            for (int row = worksheet.Dimension.Start.Row + 1; row <= worksheet.Dimension.End.Row; row++)
            {
                if (string.IsNullOrEmpty(worksheet.Cells[row, 1].Text)) // Ví dụ kiểm tra
                    continue;  // Đảm bảo continue nằm trong vòng lặp
                var employee = new Employee
                {
                    ID = worksheet.Cells[row, 1].Text, // Cột 1: ID
                    Name = worksheet.Cells[row, 2].Text, // Cột 2: Name
                    Age = int.TryParse(worksheet.Cells[row, 3].Text, out int age) ? age : 0, // Cột 3: Age
                    Income = double.TryParse(worksheet.Cells[row, 4].Text, out double income) ? income : 0.0, // Cột 4: Income
                    TaxCoe = double.TryParse(worksheet.Cells[row, 5].Text, out double taxCoe) ? taxCoe : 0.0, // Cột 5: TaxCoe
                    JobTitle = worksheet.Cells[row, 6].Text, // Cột 6: JobTitle
                    Company = worksheet.Cells[row, 7].Text // Cột 7: Company
                };
                Employees.Add(employee); // Thêm dữ liệu vào ObservableCollection
            }
        }

        public ObservableCollection<object> SheetDatas
        {
            get => Model.SheetDatas;
            set
            {
                Model.SheetDatas = value;
                OnPropertyChanged();
            }
        }

        public void Display()
        {
            if (SelectedSheetData == "Student")
                SheetDatas = new ObservableCollection<object>(Students.Cast<object>());
            else if (SelectedSheetData == "Teacher")
                SheetDatas = new ObservableCollection<object>(Teachers.Cast<object>());
            else if (SelectedSheetData == "Employee")
                SheetDatas = new ObservableCollection<object>(Employees.Cast<object>());
        }

        private bool CanExportFile(object obj) => true;

        private void ExportToExcel(object parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var package = new ExcelPackage())
                {
                    // Xuất dữ liệu của bảng Students
                    if (Students != null && Students.Count > 0)
                    {
                        var studentWorksheet = package.Workbook.Worksheets.Add("Students");
                        studentWorksheet.Cells["A1"].LoadFromCollection(Students, true); // Dữ liệu từ Students
                    }

                    // Xuất dữ liệu của bảng Teachers
                    if (Teachers != null && Teachers.Count > 0)
                    {
                        var teacherWorksheet = package.Workbook.Worksheets.Add("Teachers");
                        teacherWorksheet.Cells["A1"].LoadFromCollection(Teachers, true); // Dữ liệu từ Teachers
                    }

                    // Xuất dữ liệu của bảng Employees
                    if (Employees != null && Employees.Count > 0)
                    {
                        var employeeWorksheet = package.Workbook.Worksheets.Add("Employees");
                        employeeWorksheet.Cells["A1"].LoadFromCollection(Employees, true); // Dữ liệu từ Employees
                    }

                    // Lưu file Excel
                    package.SaveAs(new FileInfo(saveFileDialog.FileName));
                }

                MessageBox.Show("Data exported successfully.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }



    }
}