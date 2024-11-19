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
using Bai2_WPF.Command;
using Bai2_WPF.Model;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace Bai2_WPF.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public MainModel Model { get; private set; }

        public string FilePath
        {
            get => Model.FilePath;
            set
            {
                Model.FilePath = value;
                OnPropertyChanged();
            }
        }
        public string SelectedSheet
        {
            get => Model.SelectedSheet;
            set
            {
                Model.SelectedSheet = value;
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

        public ICommand ImportData { get; set; }
        public ICommand ExportData { get; set; }

        public MainViewModel(MainModel model)
        {
            Model = model;
            ImportData = new RelayCommand(ImportFile, CanImportFile);
            ExportData = new RelayCommand(ExportFile, CanExportFile);
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                Students.Clear();
                Teachers.Clear();
                Employees.Clear();
                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    DataTable dt = new DataTable();
                    dt = worksheet.Cells[worksheet.Dimension.Start.Row, worksheet.Dimension.Start.Column,
                                    worksheet.Dimension.End.Row, worksheet.Dimension.End.Column].ToDataTable();
                    dt.TableName = worksheet.Name;
                    LoadData(dt);
                    SheetNames.Add(dt.TableName);
                }
                SelectedSheet = SheetNames.FirstOrDefault();
            }
        }
        public ObservableCollection<object> CurrentObject
        {
            get => Model.CurrentObject;
            set
            {
                Model.CurrentObject = value;
                OnPropertyChanged();
            }
        }
        public void Display()
        {
            if (SelectedSheet == "Student")
            {
                CurrentObject = new ObservableCollection<object>(Students.Cast<object>());
            }
            else if (SelectedSheet == "Teacher")
            {
                CurrentObject = new ObservableCollection<object>(Teachers.Cast<object>());
            }
            else
            {
                CurrentObject = new ObservableCollection<object>(Employees.Cast<object>());
            }
        }
        private T MapDataRowToObject<T>(DataRow row) where T : new()
        {
            T obj = new T();
            foreach (var property in typeof(T).GetProperties())
            {
                if (row.Table.Columns.Contains(property.Name) && property.CanWrite)
                {
                    var value = row[property.Name];
                    if (value != DBNull.Value)
                    {
                        property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                    }
                }
            }
            return obj;
        }
        private void LoadData(DataTable dt)
        {
            string name = dt.TableName;
            if (name == "Student")
            {
                foreach (DataRow row in dt.Rows)
                {
                    Students.Add(MapDataRowToObject<Student>(row));
                }
            }
            else if (name == "Teacher")
            {
                foreach (DataRow row in dt.Rows)
                {
                    var teacher = MapDataRowToObject<Teacher>(row);
                    teacher.TaxCoe = TaxData.GetTaxCoe(teacher.Age, teacher.Income);
                    teacher.Tax = teacher.GetTax();
                    Teachers.Add(teacher);
                }
            }
            else if (name == "Employee")
            {
                foreach (DataRow row in dt.Rows)
                {
                    var employee = MapDataRowToObject<Employee>(row);
                    employee.TaxCoe = TaxData.GetTaxCoe(employee.Age, employee.Income);
                    employee.Tax = employee.GetTax();
                    Employees.Add(employee);
                }
            }
        }
        private DataTable ConvertToDataTable<T>(IEnumerable<T> data, string tableName)
        {
            DataTable dt = new DataTable(tableName);
            foreach (var property in typeof(T).GetProperties())
            {
                dt.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }
            foreach (var item in data)
            {
                DataRow row = dt.NewRow();
                foreach (var property in typeof(T).GetProperties())
                {
                    row[property.Name] = property.GetValue(item);
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
        private List<DataTable> dataTables = new List<DataTable>();
        private void importDataTables()
        {
            dataTables.Clear();

            if (Students.Any())
            {
                dataTables.Add(ConvertToDataTable(Students, "Student"));
            }

            if (Teachers.Any())
            {
                dataTables.Add(ConvertToDataTable(Teachers, "Teacher"));
            }

            if (Employees.Any())
            {
                dataTables.Add(ConvertToDataTable(Employees, "Employee"));
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
                    foreach (DataTable dt in dataTables)
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