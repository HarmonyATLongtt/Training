using Bai_1.Model;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Bai_1.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        public ObservableCollection<Person> _selection = new ObservableCollection<Person>();

        public ObservableCollection<Person> Selection
        {
            get => _selection;
            set
            {
                _selection = value;
                OnPropertyChanged(nameof(Selection));
            }
        }

        public ObservableCollection<Person> _students = new ObservableCollection<Person>();

        //public ObservableCollection<Person> Students
        //{
        //    get => _students;
        //    set
        //    {
        //        Students = value;
        //        OnPropertyChanged(nameof(Students));
        //    }
        //}

        public ObservableCollection<Person> _teachers = new ObservableCollection<Person>();

        //public ObservableCollection<Person> Teachers
        //{
        //    get => _teachers;
        //    set
        //    {
        //        Teachers = value;
        //        OnPropertyChanged(nameof(Teachers));
        //    }
        //}

        public ObservableCollection<Person> _employees = new ObservableCollection<Person>();

        //public ObservableCollection<Person> Employees
        //{
        //    get => _employees;
        //    set
        //    {
        //        Employees = value;
        //        OnPropertyChanged(nameof(Employees));
        //    }
        //}

        public ICommand OpenFileCommand { get; set; }
        public ICommand ExportFileCommand { get; set; }
        public ICommand SelectionSheet1Command { get; set; }
        public ICommand SelectionSheet2Command { get; set; }
        public ICommand SelectionSheet3Command { get; set; }
        public ICommand ClearCommand { get; set; }

        public MainViewModel()
        {
            OpenFileCommand = new RelayCommand<object>(OpenFileDialog);
            Selection = _students;
            SelectionSheet1Command = new RelayCommand<object>(SelectionSheet1);
            SelectionSheet2Command = new RelayCommand<object>(SelectionSheet2);
            SelectionSheet3Command = new RelayCommand<object>(SelectionSheet3);
            ExportFileCommand = new RelayCommand<object>(ExportFile);
            ClearCommand = new RelayCommand<object>(Clear);
        }

        public void ExportFile(object obj)
        {
            string filePath = "";
            SaveFileDialog exportFile = new SaveFileDialog();
            exportFile.Filter = "Excel files (*.xlsx)|*.xlsx|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            exportFile.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (exportFile.ShowDialog() == true)
            {
                filePath = exportFile.FileName;
            }
            else if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Đường dẫn không hợp lệ");
                return;
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage package = new ExcelPackage();
            var sheet1 = package.Workbook.Worksheets.Add("sheet1");
            int row = 1;
            int col = 1;
            foreach (var item in Selection)
            {
                col = 1;
                row++;
                //gán giá trị cho từng cell
                sheet1.Cells[row, col++].Value = item.ID;
                sheet1.Cells[row, col++].Value = item.Ten;
                sheet1.Cells[row, col++].Value = item.Tuoi;
                sheet1.Cells[row, col++].Value = item.DiaChi;
                sheet1.Cells[row, col++].Value = item.ThuNhap;
                sheet1.Cells[row, col++].Value = item.HeSoThue;
            }
            byte[] bin = package.GetAsByteArray();
            File.WriteAllBytes(filePath, bin);
        }

        public void Clear(object obj)
        {
            _students.Clear();
            _teachers.Clear();
            _employees.Clear();
        }

        public void SelectionSheet1(object obj)
        {
            Selection = _students;
        }

        public void SelectionSheet2(object obj)
        {
            Selection = _teachers;
        }

        public void SelectionSheet3(object obj)
        {
            Selection = _employees;
        }

        public void OpenFileDialog(object obj)
        {
            string filePath = "";
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
            }
            else if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Đường dẫn không hợp lệ");
                return;
            }
            var package = new ExcelPackage(new FileInfo(filePath));
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            ExcelWorksheet workSheetStudent = package.Workbook.Worksheets[0];
            for (int i = workSheetStudent.Dimension.Start.Row + 1; i <= workSheetStudent.Dimension.End.Row; i++)
            {
                int j = 1;
                string id = workSheetStudent.Cells[i, j++].Value.ToString();
                string ten = workSheetStudent.Cells[i, j++].Value.ToString();
                string tuoi = workSheetStudent.Cells[i, j++].Value.ToString();
                string diaChi = workSheetStudent.Cells[i, j++].Value.ToString();
                string thuNhap = workSheetStudent.Cells[i, j++].Value.ToString();
                string hesoThue = workSheetStudent.Cells[i, j++].Value.ToString();
                Person dataStudent = new Person()
                {
                    ID = id,
                    Ten = ten,
                    Tuoi = tuoi,
                    DiaChi = diaChi,
                    ThuNhap = thuNhap,
                    HeSoThue = hesoThue
                };
                _students.Add(dataStudent);
            }

            ExcelWorksheet workSheetTeacher = package.Workbook.Worksheets[1];
            for (int i = workSheetTeacher.Dimension.Start.Row + 1; i <= workSheetTeacher.Dimension.End.Row; i++)
            {
                int j = 1;
                string id = workSheetTeacher.Cells[i, j++].Value.ToString();
                string ten = workSheetTeacher.Cells[i, j++].Value.ToString();
                string tuoi = workSheetTeacher.Cells[i, j++].Value.ToString();
                string diaChi = workSheetTeacher.Cells[i, j++].Value.ToString();
                string thuNhap = workSheetTeacher.Cells[i, j++].Value.ToString();
                string hesoThue = workSheetTeacher.Cells[i, j++].Value.ToString();
                Person dataTeacher = new Person()
                {
                    ID = id,
                    Ten = ten,
                    Tuoi = tuoi,
                    DiaChi = diaChi,
                    ThuNhap = thuNhap,
                    HeSoThue = hesoThue
                };
                _teachers.Add(dataTeacher);
            }

            ExcelWorksheet workSheetEmployees = package.Workbook.Worksheets[2];
            for (int i = workSheetEmployees.Dimension.Start.Row + 1; i <= workSheetEmployees.Dimension.End.Row; i++)
            {
                int j = 1;
                string id = workSheetEmployees.Cells[i, j++].Value.ToString();
                string ten = workSheetEmployees.Cells[i, j++].Value.ToString();
                string tuoi = workSheetEmployees.Cells[i, j++].Value.ToString();
                string diaChi = workSheetEmployees.Cells[i, j++].Value.ToString();
                string thuNhap = workSheetEmployees.Cells[i, j++].Value.ToString();
                string hesoThue = workSheetEmployees.Cells[i, j++].Value.ToString();
                Person dataEmployees = new Person()
                {
                    ID = id,
                    Ten = ten,
                    Tuoi = tuoi,
                    DiaChi = diaChi,
                    ThuNhap = thuNhap,
                    HeSoThue = hesoThue
                };
                _employees.Add(dataEmployees);
            }
        }
    }
}