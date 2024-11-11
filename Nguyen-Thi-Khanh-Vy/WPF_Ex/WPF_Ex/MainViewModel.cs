using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Windows;
using System.Windows.Input;
using LicenseContext = OfficeOpenXml.LicenseContext;
using WPF_Ex.Model;

namespace WPF_Ex
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private MainModel _mainModel;

        // Thuộc tính FileName sẽ gán và lấy giá trị từ _mainModel.FilePath
        public string FileName
        {
            get => _mainModel.FilePath;
            set
            {
                _mainModel.FilePath = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        // Thuộc tính People sẽ gán và lấy giá trị từ _mainModel.People
        public ObservableCollection<object> People
        {
            get => _mainModel.People;
            set
            {
                _mainModel.People = value;
                OnPropertyChanged(nameof(People));
            }
        }

        // Thuộc tính SelectedPerson sẽ gán và lấy giá trị từ _mainModel.SelectedPerson
        public object SelectedPerson
        {
            get => _mainModel.SelectedPerson;
            set
            {
                _mainModel.SelectedPerson = value;
                OnPropertyChanged(nameof(SelectedPerson));
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand ExportCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _mainModel = new MainModel(); // Khởi tạo MainModel
            LoadCommand = new RelayCommand(LoadExcelFile); // Command để tải file Excel
            ExportCommand = new RelayCommand(ExportToExcel); // Command để xuất dữ liệu ra Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Load file Excel và cập nhật danh sách People
        private void LoadExcelFile(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "Excel Files|*.xlsx;*.xls" };
            if (openFileDialog.ShowDialog() == true)
            {
                People.Clear();
                FileName = openFileDialog.FileName;

                try
                {
                    using (var package = new ExcelPackage(new FileInfo(FileName)))
                    {
                        foreach (var sheet in package.Workbook.Worksheets)
                        {
                            var dim = sheet.Dimension;
                            if (dim == null) continue;

                            for (int row = sheet.Dimension.Start.Row + 1; row <= sheet.Dimension.End.Row; row++)
                            {
                                var person = CreatePersonFromRow(sheet, row, sheet.Name);
                                if (person != null)
                                {
                                    People.Add(person); // Thêm đối tượng vào danh sách People
                                }
                            }
                        }

                        // Cập nhật SelectedPerson sau khi dữ liệu đã được load
                        SelectedPerson = People.FirstOrDefault();

                        // Thông báo thành công
                        MessageBox.Show($"{People.Count} people loaded successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu có
                    MessageBox.Show($"An error occurred while loading the Excel file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Hàm tạo đối tượng Person từ dữ liệu trong một dòng Excel
        private object CreatePersonFromRow(ExcelWorksheet sheet, int row, string sheetName)
        {
            object person = null;

            switch (sheetName.ToLower())
            {
                case "employee":
                    person = new Employee
                    {
                        ID = sheet.Cells[row, 1].Text,
                        Name = sheet.Cells[row, 2].Text,
                        Age = int.TryParse(sheet.Cells[row, 3].Text, out var ageEmp) ? ageEmp : 0,
                        Income = double.TryParse(sheet.Cells[row, 4].Text, out var incomeEmp) ? incomeEmp : 0,
                        TaxCoe = double.TryParse(sheet.Cells[row, 5].Text, out var taxcoeEmp) ? taxcoeEmp : 0,
                        JobTitle = sheet.Cells[row, 6].Text,
                        Company = sheet.Cells[row, 7].Text
                    };

                    break;

                case "student":
                    person = new Student
                    {
                        ID = sheet.Cells[row, 1].Text,
                        Name = sheet.Cells[row, 2].Text,
                        Age = int.TryParse(sheet.Cells[row, 3].Text, out var ageStu) ? ageStu : 0,
                        Class = sheet.Cells[row, 8].Text, // Kiểm tra lại chỉ số cột cho Class
                        School = sheet.Cells[row, 9].Text // Kiểm tra lại chỉ số cột cho School
                    };

                    break;

                case "teacher":
                    person = new Teacher
                    {
                        ID = sheet.Cells[row, 1].Text,
                        Name = sheet.Cells[row, 2].Text,
                        Age = int.TryParse(sheet.Cells[row, 3].Text, out var ageTea) ? ageTea : 0,
                        School = sheet.Cells[row, 4].Text,
                        Income = double.TryParse(sheet.Cells[row, 5].Text, out var incomeTea) ? incomeTea : 0,
                        TaxCoe = double.TryParse(sheet.Cells[row, 6].Text, out var taxcoeTea) ? taxcoeTea : 0
                    };

                    break;

                default:
                    return null;
            }

            return person;
        }



        private void ExportToExcel(object parameter)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "Excel Files|*.xlsx" };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("PeopleData");
                        worksheet.Cells[1, 1].Value = "ID";
                        worksheet.Cells[1, 2].Value = "Name";
                        worksheet.Cells[1, 3].Value = "Age";
                        worksheet.Cells[1, 4].Value = "JobTitle";
                        worksheet.Cells[1, 5].Value = "Company";
                        worksheet.Cells[1, 6].Value = "Income";
                        worksheet.Cells[1, 7].Value = "TaxCoe";
                        worksheet.Cells[1, 8].Value = "Class";  // Cột Class cho Student
                        worksheet.Cells[1, 9].Value = "School"; // Cột School cho Student

                        int row = 2;
                        foreach (var person in People)
                        {
                            worksheet.Cells[row, 1].Value = ((Person)person).ID;
                            worksheet.Cells[row, 2].Value = ((Person)person).Name;
                            worksheet.Cells[row, 3].Value = ((Person)person).Age;

                            if (person is Student student)
                            {
                                worksheet.Cells[row, 8].Value = student.Class;
                                worksheet.Cells[row, 9].Value = student.School;
                            }
                            else if (person is Teacher teacher)
                            {
                                worksheet.Cells[row, 9].Value = teacher.School;
                                worksheet.Cells[row, 6].Value = teacher.Income;
                                worksheet.Cells[row, 7].Value = teacher.TaxCoe;
                            }
                            else if (person is Employee employee)
                            {
                                worksheet.Cells[row, 6].Value = employee.Income;
                                worksheet.Cells[row, 7].Value = employee.TaxCoe;
                                worksheet.Cells[row, 4].Value = employee.JobTitle;
                                worksheet.Cells[row, 5].Value = employee.Company;
                            }

                            row++;
                        }

                        FileInfo excelFile = new FileInfo(saveFileDialog.FileName);
                        package.SaveAs(excelFile);
                        MessageBox.Show("File exported successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while exporting to Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
