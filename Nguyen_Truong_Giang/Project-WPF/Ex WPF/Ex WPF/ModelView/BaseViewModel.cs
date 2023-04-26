using ExcelDataReader;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using OfficeOpenXml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Input;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace Ex_WPF.ModelView
{
    class BaseViewModel : MainViewModel
    {
        public ObservableCollection<Person> _selection = new ObservableCollection<Person>();

        public string sheetName = "Sheet1";
        public string sheetStudent = "Student";
        public string sheetTeacher = "Teacher";
        public string sheetEmployee = "Employees";
        public ObservableCollection<Person> Selection
        {
            get => _selection;
            set
            {
                _selection = value;
                RaisePropertiesChanged(nameof(Selection));
            }
        }

        public ObservableCollection<Person> _student = new ObservableCollection<Person>();

        public ObservableCollection<Person> _teacher = new ObservableCollection<Person>();

        public ObservableCollection<Person> _employee = new ObservableCollection<Person>();

        public ICommand ImportFileCommand { get; set; }
        public ICommand ExportFileCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand SelectionSheet1Command { get; set; }
        public ICommand SelectionSheet2Command { get; set; }
        public ICommand SelectionSheet3Command { get; set; }

        public BaseViewModel()
        {
            ImportFileCommand = new RelayCommand<object>(ImportFile);
            ExportFileCommand = new RelayCommand<object>(ExportFile);
            SelectionSheet1Command = new RelayCommand<object>(SelectionSheet1);
            SelectionSheet2Command = new RelayCommand<object>(SelectionSheet2);
            SelectionSheet3Command = new RelayCommand<object>(SelectionSheet3);
            ClearCommand = new RelayCommand<object>(Clear);

            //SheetName = "Students";
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
                        DataTable sheet = dataSet.Tables[sheetName];

                        // Đọc dữ liệu từ sheet vào DataTable
                        reader.Read();


                        var Student = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        }).Tables["Student"];

                        var bindingList = new BindingList<Person>();

                        // Lấy ra list Student và lưu vào biến
                        for (int indexRow = 0; indexRow < Student.Rows.Count; indexRow++)
                        {
                            DataRow row = Student.Rows[indexRow];
                            Person student = new Person();
                            student.ID = row[0].ToString();
                            student.Name = row[1].ToString();
                            student.Age = int.Parse(row[2].ToString());
                            student.Address = row[3].ToString();
                            student.TaxFactor = double.Parse(row[4].ToString());

                            bindingList.Add(student);

                            Person studentss = new Person()
                            {
                                ID = student.ID,
                                Name = student.Name,
                                Age = student.Age,
                                Address = student.Address,
                                TaxFactor = student.TaxFactor,
                            };
                            _student.Add(studentss);
                        }


                        // Lấy ra list Teacher và lưu vào biến
                        var Teacher = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        }).Tables["Teacher"];


                        for (int indexRow = 0; indexRow < Teacher.Rows.Count; indexRow++)
                        {
                            DataRow row = Teacher.Rows[indexRow];
                            Person teacher = new Person();
                            teacher.ID = row[0].ToString();
                            teacher.Name = row[1].ToString();
                            teacher.Age = int.Parse(row[2].ToString());
                            teacher.Address = row[3].ToString();
                            teacher.TaxFactor = double.Parse(row[4].ToString());

                            bindingList.Add(teacher);

                            Person teachers = new Person()
                            {
                                ID = teacher.ID,
                                Name = teacher.Name,
                                Age = teacher.Age,
                                Address = teacher.Address,
                                TaxFactor = teacher.TaxFactor,
                            };
                            _teacher.Add(teachers);
                        }

                        // Lấy ra list Employee và lưu vào biến
                        var Employees = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        }).Tables["Employees"];


                        for (int indexRow = 0; indexRow < Employees.Rows.Count; indexRow++)
                        {
                            DataRow row = Employees.Rows[indexRow];
                            Person employee = new Person();
                            employee.ID = row[0].ToString();
                            employee.Name = row[1].ToString();
                            employee.Age = int.Parse(row[2].ToString());
                            employee.Address = row[3].ToString();
                            employee.TaxFactor = double.Parse(row[4].ToString());

                            bindingList.Add(employee);

                            Person employees = new Person()
                            {
                                ID = employee.ID,
                                Name = employee.Name,
                                Age = employee.Age,
                                Address = employee.Address,
                                TaxFactor = employee.TaxFactor,
                            };
                            _employee.Add(employees);
                        }


                        // Binding dữ liệu lên ListView
                        //studentsDataGrid.ItemsSource = dt.DefaultView; 
                    }
                }

                Selection = _student;
            }
        }

        public void ExportFile(object obj)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel Files|*.xlsx;*.xls;*.xlsm";

            if (saveFileDialog.ShowDialog() == true)
            {
                using (ExcelPackage package = new ExcelPackage())
                {
                    if (Selection == _student)
                    {
                        ExcelWorksheet studentSheet = package.Workbook.Worksheets.Add("Student");
                        studentSheet.Cells[1, 1].Value = "ID";
                        studentSheet.Cells[1, 2].Value = "Name";
                        studentSheet.Cells[1, 3].Value = "Age";
                        studentSheet.Cells[1, 4].Value = "Address";
                        studentSheet.Cells[1, 5].Value = "TaxFactor";

                        int rowIndex = 2;
                        foreach (Person student in Selection)
                        {
                            studentSheet.Cells[rowIndex, 1].Value = student.ID;
                            studentSheet.Cells[rowIndex, 2].Value = student.Name;
                            studentSheet.Cells[rowIndex, 3].Value = student.Age;
                            studentSheet.Cells[rowIndex, 4].Value = student.Address;
                            studentSheet.Cells[rowIndex, 5].Value = student.TaxFactor;
                            rowIndex++;
                        }
                    }
                    else if (Selection == _teacher)
                    {
                        ExcelWorksheet teacherSheet = package.Workbook.Worksheets.Add("Teacher");
                        teacherSheet.Cells[1, 1].Value = "ID";
                        teacherSheet.Cells[1, 2].Value = "Name";
                        teacherSheet.Cells[1, 3].Value = "Age";
                        teacherSheet.Cells[1, 4].Value = "Address";
                        teacherSheet.Cells[1, 5].Value = "TaxFactor";

                        int rowIndex = 2;
                        foreach (Person teacher in Selection)
                        {
                            teacherSheet.Cells[rowIndex, 1].Value = teacher.ID;
                            teacherSheet.Cells[rowIndex, 2].Value = teacher.Name;
                            teacherSheet.Cells[rowIndex, 3].Value = teacher.Age;
                            teacherSheet.Cells[rowIndex, 4].Value = teacher.Address;
                            teacherSheet.Cells[rowIndex, 5].Value = teacher.TaxFactor;
                            rowIndex++;
                        }
                    }
                    else if (Selection == _employee)
                    {
                        ExcelWorksheet employeeSheet = package.Workbook.Worksheets.Add("Employees");
                        employeeSheet.Cells[1, 1].Value = "ID";
                        employeeSheet.Cells[1, 2].Value = "Name";
                        employeeSheet.Cells[1, 3].Value = "Age";
                        employeeSheet.Cells[1, 4].Value = "Address";
                        employeeSheet.Cells[1, 5].Value = "TaxFactor";

                        int rowIndex = 2;
                        foreach (Person employees in Selection)
                        {
                            employeeSheet.Cells[rowIndex, 1].Value = employees.ID;
                            employeeSheet.Cells[rowIndex, 2].Value = employees.Name;
                            employeeSheet.Cells[rowIndex, 3].Value = employees.Age;
                            employeeSheet.Cells[rowIndex, 4].Value = employees.Address;
                            employeeSheet.Cells[rowIndex, 5].Value = employees.TaxFactor;
                            rowIndex++;
                        }
                    }

                    package.SaveAs(new FileInfo(saveFileDialog.FileName));
                }

                MessageBox.Show("Export successful!");
            }
            else
            {
                MessageBox.Show("Err!");
                return;
            }
        }

        public void SelectionSheet1(object obj)
        {
            Selection = _student;
        }

        public void SelectionSheet2(object obj)
        {
            Selection = _teacher;
        }

        public void SelectionSheet3(object obj)
        {
            Selection = _employee;
        }

        public void Clear(object obj)
        {
            _student.Clear();
            _teacher.Clear();
            _employee.Clear();
            MessageBox.Show("Clear successful!");
        }
    }
}
