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
            get =>Model.SelectedSheet;
            set
            {
                Model.SelectedSheet = value; 
                OnPropertyChanged();
                Display();
            }
        }
        public ObservableCollection<string> SheetNames
        {
            get =>Model.SheetNames;
            set
            {
                Model.SheetNames = value;
                OnPropertyChanged();
            }
        }
        //public DataTable SelectedData
        //{
        //    get => Model.SelectedData;
        //    set
        //    {
        //        Model.SelectedData = value;
        //        OnPropertyChanged();
        //        Display();
        //    }
        //}

        //public ObservableCollection<DataTable> Datas
        //{
        //    get => Model.Datas;
        //    set
        //    {
        //        Model.Datas = value;
        //        OnPropertyChanged();
        //    }
        //}
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
        private void LoadData(DataTable dt)
        {
            string name = dt.TableName;
            if (name == "Student")
            {
                string id="", ten="", lop="", truong = "";
                int age=0;
                foreach (DataRow row in dt.Rows)
                {
                    foreach(DataColumn cl in dt.Columns)
                    {
                        if (cl.ColumnName == "ID")
                        {
                            id = row[cl].ToString();
                        }
                        if (cl.ColumnName == "Name")
                        {
                            ten = row[cl].ToString();
                        }
                        if (cl.ColumnName == "Age")
                        {
                            age = int.Parse(row[cl].ToString());
                        }
                        if (cl.ColumnName == "Class")
                        {
                            lop = row[cl].ToString();
                        }
                        if (cl.ColumnName == "School")
                        {
                            truong = row[cl].ToString();
                        }
                    }
                    Student st = new Student(id, ten, age, lop, truong);
                    Students.Add(st);
                }
            }
            else if (name == "Teacher")
            {
                string id = "", ten = "", truong = "";
                int age = 0, luong=0;
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn cl in dt.Columns)
                    {
                        if (cl.ColumnName == "ID")
                        {
                            id = row[cl].ToString();
                        }
                        if (cl.ColumnName == "Name")
                        {
                            ten = row[cl].ToString();
                        }
                        if (cl.ColumnName == "Age")
                        {
                            age = int.Parse(row[cl].ToString());
                        }
                        if (cl.ColumnName == "School")
                        {
                            truong = row[cl].ToString();
                        }
                        if(cl.ColumnName == "Income")
                        {
                            luong = int.Parse(row[cl].ToString());
                        }
                    }
                    Teacher tc = new Teacher(id, ten, age, truong, luong);
                    Teachers.Add(tc);
                }
            }
            else
            {
                string id = "", ten = "", job = "";
                int age = 0, luong = 0;
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn cl in dt.Columns)
                    {
                        if (cl.ColumnName == "ID")
                        {
                            id = row[cl].ToString();
                        }
                        if (cl.ColumnName == "Name")
                        {
                            ten = row[cl].ToString();
                        }
                        if (cl.ColumnName == "Age")
                        {
                            age = int.Parse(row[cl].ToString());
                        }
                        if (cl.ColumnName == "Job")
                        {
                            job = row[cl].ToString();
                        }
                        if (cl.ColumnName == "Income")
                        {
                            luong = int.Parse(row[cl].ToString());
                        }
                    }
                    Employee em = new Employee(id, ten, age, job, luong);
                    Employees.Add(em);
                }
            }
        }
        private List<DataTable> dataTables = new List<DataTable>();
        private void importDataTables()
        {
            dataTables.Clear();

            if (Students.Any())
            {
                DataTable dT = new DataTable("Student");
                dT.Columns.Add("ID");
                dT.Columns.Add("Name");
                dT.Columns.Add("Age");
                dT.Columns.Add("Class");
                dT.Columns.Add("School");

                foreach (Student st in Students)
                {
                    DataRow row = dT.NewRow();
                    row["ID"] = st.ID;
                    row["Name"] = st.Name;
                    row["Age"] = st.Age;
                    row["Class"] = st.Class;
                    row["School"] = st.School;
                    dT.Rows.Add(row);
                }
                dataTables.Add(dT);
            }

            if (Teachers.Any())
            {
                DataTable dT = new DataTable("Teacher");
                dT.Columns.Add("ID");
                dT.Columns.Add("Name");
                dT.Columns.Add("Age");
                dT.Columns.Add("School");
                dT.Columns.Add("Income");
                dT.Columns.Add("Tax Coe");
                dT.Columns.Add("Tax");

                foreach (Teacher tc in Teachers)
                {
                    DataRow row = dT.NewRow();
                    row["ID"] = tc.ID;
                    row["Name"] = tc.Name;
                    row["Age"] = tc.Age;
                    row["School"] = tc.School;
                    row["Income"] = tc.Income;
                    row["Tax Coe"] = tc.TaxCoe;
                    row["Tax"] = tc.Tax;
                    dT.Rows.Add(row);
                }
                dataTables.Add(dT);
            }

            if (Employees.Any())
            {
                DataTable dT = new DataTable("Employee");
                dT.Columns.Add("ID");
                dT.Columns.Add("Name");
                dT.Columns.Add("Age");
                dT.Columns.Add("Job");
                dT.Columns.Add("Income");
                dT.Columns.Add("Tax Coe");
                dT.Columns.Add("Tax");
                foreach (Employee em in Employees)
                {
                    DataRow row = dT.NewRow();
                    row["ID"] = em.ID;
                    row["Name"] = em.Name;
                    row["Age"] = em.Age;
                    row["Job"] = em.Job;
                    row["Income"] = em.Income;
                    row["Tax Coe"] = em.TaxCoe;
                    row["Tax"] = em.Tax;
                    dT.Rows.Add(row);
                }
                dataTables.Add(dT);
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