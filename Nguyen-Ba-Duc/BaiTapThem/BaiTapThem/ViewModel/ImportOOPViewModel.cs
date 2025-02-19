using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaiTapThem.Model;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using OfficeOpenXml;
using System.IO;
using System.Windows;
using System.Data;
using System.Reflection;
using System.Security.Claims;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection.PortableExecutable;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace BaiTapThem.ViewModel
{
    public class ImportOOPViewModel : BaseViewModel
    {
        public ObservableCollection<string> Headers { get; set; }
        public ObservableCollection<ObservableCollection<string>> ListHeaders { get; set; }

        private ObservableCollection<string> _comboboxItems;

        public ObservableCollection<string> ComboboxItems
        {
            get { return _comboboxItems; }
            set
            {
                _comboboxItems = value;

                OnPropertyChanged();
            }
        }

        private string _selectedValue;

        public string SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                _selectedValue = value;
                LoadPerson();
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Person> _students;

        private ObservableCollection<Person> _teachers;

        private ObservableCollection<Person> _employees;

        //public DataGrid DataGrid { get; set; }

        //public DataGrid DataGrid
        //{
        //    get { return _dataGrid; }
        //    set
        //    {
        //        _dataGrid = value;
        //        OnPropertyChanged();
        //    }
        //}

        private ObservableCollection<Person> _persons;

        public ObservableCollection<Person> Persons
        {
            get { return _persons; }
            set
            {
                _persons = value;

                OnPropertyChanged();
            }
        }

        private string filePath = "";

        public RelayCommand ImportCommand { get; set; }
        public RelayCommand<object> GenerateColumnsCommand { get; set; }

        public ImportOOPViewModel()
        {
            ImportCommand = new RelayCommand(ImportFile);
            GenerateColumnsCommand = new RelayCommand<object>(GenerateColumns);
            Persons = new ObservableCollection<Person>();
            ListHeaders = new ObservableCollection<ObservableCollection<string>>();
            Headers = new ObservableCollection<string>();

            //Person person = new Student(1, "Duc", 31, "12A2", "Gia Binh 1");

            //string str = "";
            //foreach (var prop in GetProperty(person))
            //{
            //    str += prop.Name + " ," + prop.Value + " ," + prop.Type + "\n";
            //}
            //MessageBox.Show(str);

            //Type personType = typeof(Student);
            //Type personType = person.GetType();

            // Lấy thông tin về các trường
            //foreach (PropertyInfo property in personType.GetProperties())
            //{
            //    MessageBox.Show($"Tên trường: {property.Name}, Giá trị: {property.GetValue(person)} , Kiểu dữ liệu: {property.PropertyType}");
            //}
        }

        private void GenerateColumns(object p)
        {
            if (p is not DataGrid dataGrid) return;
            if (Headers == null) return;

            dataGrid.Columns.Clear();

            foreach (var header in Headers)
            {
                dataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = header,
                    Binding = new Binding(header)
                });
            }
        }

        private void ImportFile(object p)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("The path is invalid");
            }
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            var package = new ExcelPackage(new FileInfo(filePath));

            List<String> listSheetName = new List<String>();
            var worksheets = package.Workbook.Worksheets;
            for (int a = 0; a < worksheets.Count; a++)
            {
                var worksheet = worksheets[a];
                listSheetName.Add(worksheet.Name);

                var totalRows = worksheet.Dimension.End.Row;

                var totalColumns = worksheet.Dimension.End.Column;

                int startRow = 0;
                int startCol = 0;

                for (int i = 1; i <= totalRows; i++)
                {
                    for (int j = 1; j <= totalColumns; j++)
                    {
                        if (worksheet.Cells[i, j].Value != null)
                        {
                            startRow = i; startCol = j;
                            break;
                        }
                    }
                    if (startRow != 0) break;
                }

                if (worksheet.Name == "Student")
                {
                    //items.Add(new ItemWithImage { Text = worksheet.Name, Image = new BitmapImage(new Uri("C:\\Users\\PC\\Downloads\\student.png")) });

                    Dictionary<int, string> listTitles = new Dictionary<int, string>();
                    for (int s = startCol; s <= totalColumns; s++)
                    {
                        listTitles.Add(s, worksheet.Cells[startRow, s].Value?.ToString());
                    }
                    int idCol = 0, nameCol = 0, ageCol = 0, classCol = 0, schoolCol = 0;
                    int id = 0, age = 0;
                    string name = "", school = "", lop = "";

                    foreach (var title in listTitles)
                    {
                        if (title.Value == "Id")
                        {
                            idCol = title.Key;
                        }
                        if (title.Value == "Name")
                        {
                            nameCol = title.Key;
                        }
                        if (title.Value == "Age")
                        {
                            ageCol = title.Key;
                        }
                        if (title.Value == "Class")
                        {
                            classCol = title.Key;
                        }
                        if (title.Value == "School")
                        {
                            schoolCol = title.Key;
                        }
                    }

                    List<Student> liststudents = new List<Student>();
                    for (int i = startRow + 1; i <= totalRows; i++)
                    {
                        if (worksheet.Cells[i, idCol].Value != null)
                        {
                            if (int.TryParse((worksheet.Cells[i, idCol].Value.ToString()), out int idValue))
                            {
                                id = idValue;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        if (worksheet.Cells[i, ageCol].Value != null)
                        {
                            if (int.TryParse((worksheet.Cells[i, ageCol].Value.ToString()), out int ageValue))
                            {
                                age = ageValue;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (worksheet.Cells[i, nameCol].Value != null
                            && worksheet.Cells[i, classCol].Value != null
                            && worksheet.Cells[i, schoolCol].Value != null)
                        {
                            name = worksheet.Cells[i, nameCol].Value.ToString();
                            lop = worksheet.Cells[i, classCol].Value.ToString();
                            school = worksheet.Cells[i, schoolCol].Value.ToString();
                        }
                        else
                        {
                            continue;
                        }

                        liststudents.Add(new Student
                        {
                            Id = id,
                            Name = name,
                            Age = age,
                            Class = lop,
                            School = school

                            //Id = worksheet.Cells[i, idCol].Value != null ? int.Parse(worksheet.Cells[i, idCol].Value?.ToString()) : null,
                            //Name = worksheet.Cells[i, nameCol].Value?.ToString(),
                            //Age = worksheet.Cells[i, ageCol].Value != null ? int.Parse(worksheet.Cells[i, ageCol].Value?.ToString()) : null,
                            //Class = worksheet.Cells[i, classCol].Value?.ToString(),
                            //School = worksheet.Cells[i, schoolCol].Value?.ToString()
                        });
                    }
                    _students = new ObservableCollection<Person>(liststudents);
                    ListHeaders.Add(new ObservableCollection<string>(GetPropertyNames<Student>()));
                }
                if (worksheet.Name == "Employee")
                {
                    //items.Add(new ItemWithImage { Text = worksheet.Name, Image = new BitmapImage(new Uri("C:\\Users\\PC\\Downloads\\employee.png")) });

                    List<Employee> listemployees = new List<Employee>();
                    for (int i = startRow + 1; i <= totalRows; i++)
                    {
                        listemployees.Add(new Employee
                        {
                            Id = worksheet.Cells[i, startCol].Value != null ? int.Parse(worksheet.Cells[i, startCol].Value.ToString()) : default,
                            Name = worksheet.Cells[i, startCol + 1].Value?.ToString(),
                            Age = worksheet.Cells[i, startCol + 2].Value != null ? int.Parse(worksheet.Cells[i, startCol + 2].Value.ToString()) : default,
                            Company = worksheet.Cells[i, startCol + 3].Value?.ToString(),
                            JobTitle = worksheet.Cells[i, startCol + 4].Value?.ToString(),
                            Income = worksheet.Cells[i, startCol + 5].Value != null ? int.Parse(worksheet.Cells[i, startCol + 5].Value.ToString()) : default,
                            TaxCoe = worksheet.Cells[i, startCol + 6].Value != null ? double.Parse(worksheet.Cells[i, startCol + 6].Value.ToString()) : default
                        });
                    }
                    _employees = new ObservableCollection<Person>(listemployees);
                    ListHeaders.Add(new ObservableCollection<string>(GetPropertyNames<Employee>()));
                }
                if (worksheet.Name == "Teacher")
                {
                    //items.Add(new ItemWithImage { Text = worksheet.Name, Image = new BitmapImage(new Uri("C:\\Users\\PC\\Downloads\\teacher.png")) });

                    List<Teacher> listteachers = new List<Teacher>();
                    for (int i = startRow + 1; i <= totalRows; i++)
                    {
                        listteachers.Add(new Teacher
                        {
                            Id = worksheet.Cells[i, startCol + 1].Value != null ? int.Parse(worksheet.Cells[i, startCol + 1].Value.ToString()) : default,
                            Name = worksheet.Cells[i, startCol].Value?.ToString(),
                            Age = worksheet.Cells[i, startCol + 2].Value != null ? int.Parse(worksheet.Cells[i, startCol + 2].Value.ToString()) : default,
                            School = worksheet.Cells[i, startCol + 3].Value?.ToString(),
                            Income = worksheet.Cells[i, startCol + 4].Value != null ? int.Parse(worksheet.Cells[i, startCol + 4].Value.ToString()) : default,
                            TaxCoe = worksheet.Cells[i, startCol + 5].Value != null ? double.Parse(worksheet.Cells[i, startCol + 5].Value.ToString()) : default
                        });
                    }
                    _teachers = new ObservableCollection<Person>(listteachers);
                    ListHeaders.Add(new ObservableCollection<string>(GetPropertyNames<Teacher>()));
                }
            }
            SelectedValue = "Student";
            ComboboxItems = new ObservableCollection<string>(listSheetName);
        }

        private void LoadPerson()
        {
            if (SelectedValue == "Student")
            {
                Headers = ListHeaders[0];

                Persons = _students;
                //GenerateColumns(DataGrid);
            }
            if (SelectedValue == "Teacher")
            {
                Headers = ListHeaders[1];
                Persons = _teachers;
                //GenerateColumns(DataGrid);
            }
            if (SelectedValue == "Employee")
            {
                //Persons = new ObservableCollection<Person>(_employees);
                //Headers = new ObservableCollection<string>(GetPropertyNames<Employee>());
                Headers = ListHeaders[2];
                Persons = _employees;
                //GenerateColumns(DataGrid);
            }
        }

        private List<string> GetPropertyNames<T>()
        {
            return typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(p => p.Name)
                .ToList();
        }

        //private List<(string Name, Object Value, Type Type)> GetProperty(Object obj)
        //{
        //    if (obj == null) throw new ArgumentNullException(nameof(obj));

        //    return obj.GetType()
        //    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        //    .Select(p => (p.Name, p.GetValue(obj), p.PropertyType))
        //    .ToList();
        //}
    }

    //public class ItemWithImage
    //{
    //    public string Text { get; set; }
    //    public ImageSource Image { get; set; }
    //}

    //public class ManagerPerson<T> where T : Person
    //{
    //    private List<T> _persons = new List<T>();

    //    public void Add(T person)
    //    {
    //        _persons.Add(person);
    //    }

    //    public List<T> GetAll()
    //    { return _persons; }
    //}
}