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
using System.Windows.Media;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace Ex_WPF.ModelView
{
    class BaseViewModel : MainViewModel
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

        public ObservableCollection<Person> _student = new ObservableCollection<Person>();

        public ObservableCollection<Person> _teacher = new ObservableCollection<Person>();

        public ObservableCollection<Person> _employee = new ObservableCollection<Person>();

        public ObservableCollection<Person> _nextSheet = new ObservableCollection<Person>();

        public ObservableCollection<Person> _backSheet = new ObservableCollection<Person>();


        public ICommand ImportFileCommand { get; set; }
        public ICommand ExportFileCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand SelectionSheet1Command { get; set; }
        public ICommand SelectionSheet2Command { get; set; }
        public ICommand SelectionSheet3Command { get; set; }
        public ICommand NextSheetCommand { get; set; }
        public ICommand BackSheetCommand { get; set; }

        public BaseViewModel()
        {
            ImportFileCommand = new RelayCommand<object>(ImportFile);
            ExportFileCommand = new RelayCommand<object>(ExportFile);
            SelectionSheet1Command = new RelayCommand<object>(SelectionSheet1);
            SelectionSheet2Command = new RelayCommand<object>(SelectionSheet2);
            SelectionSheet3Command = new RelayCommand<object>(SelectionSheet3);
            ClearCommand = new RelayCommand<object>(Clear);
            NextSheetCommand = new RelayCommand<object>(NextSheet);
            BackSheetCommand = new RelayCommand<object>(BackSheet);

            //SheetName = "Students";
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

                for (int indexRow = 0; indexRow < sheet.Rows.Count; indexRow++)
                {
                    DataRow row = sheet.Rows[indexRow];
                    Person person = new Person();
                    person.ID = row[0].ToString();
                    person.Name = row[1].ToString();
                    person.Age = int.Parse(row[2].ToString());
                    person.Address = row[3].ToString();
                    person.TaxFactor = double.Parse(row[4].ToString());
                    //add item into the list you want
                    bindingList.Add(person);

                    Person persons = new Person()
                    {
                        ID = person.ID,
                        Name = person.Name,
                        Age = person.Age,
                        Address = person.Address,
                        TaxFactor = person.TaxFactor,
                    };
                    _selection.Add(persons);
                }
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

                        // Đọc dữ liệu từ sheet vào DataTable
                        reader.Read();

                        sheets = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        }).Tables;

                        InitSheet(0);

                        //var bindingList = new BindingList<Person>();

                        //DataTable sheet = sheets[index];

                        //for (int indexRow = 0; indexRow < sheet.Rows.Count; indexRow++)
                        //{
                        //    DataRow row = sheet.Rows[indexRow];
                        //    Person person = new Person();
                        //    person.ID = row[0].ToString();
                        //    person.Name = row[1].ToString();
                        //    person.Age = int.Parse(row[2].ToString());
                        //    person.Address = row[3].ToString();
                        //    person.TaxFactor = double.Parse(row[4].ToString());
                        //    //add item into the list you want
                        //    bindingList.Add(person);

                        //    Person persons = new Person()
                        //    {
                        //        ID = person.ID,
                        //        Name = person.Name,
                        //        Age = person.Age,
                        //        Address = person.Address,
                        //        TaxFactor = person.TaxFactor,
                        //    };
                        //    _selection.Add(persons);
                        //}

                    }
                }

                Selection = _selection;
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

        public void NextSheet(object obj)
        {
            index++;
            if (index >= 0)
            {
                InitSheet(index);
            }
            if(index >= sheets.Count)
            {
                index = 0;
                InitSheet(index);
                MessageBox.Show("No more sheets, go back to first sheets !");
            }

        }

        public void BackSheet(object obj)
        {
            index--;
            if(index >= 0)
            {
                InitSheet(index);
            }
            if (index < 0)
            {
                index = 0;
                InitSheet(index);
                MessageBox.Show("This is the first sheet !");
            }

        }
    }
}
