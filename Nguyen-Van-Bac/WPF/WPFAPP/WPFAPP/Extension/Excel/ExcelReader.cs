using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using WPFAPP.Model;

namespace WPFAPP.Extension.Excel
{
    public class ExcelReader
    {
        public static ObservableCollection<object> ReadStudents(string filePath)
        {
            List<Student> students = new List<Student>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Student"];
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    students.Add(new Student
                    {
                        ID = worksheet.Cells[row, 1].Value.ToString(),
                        Name = worksheet.Cells[row, 2].Value.ToString(),
                        Age = Convert.ToInt32(worksheet.Cells[row, 3].Value),
                        Address = worksheet.Cells[row, 4].Value.ToString(),
                        TaxCode = Convert.ToInt32(worksheet.Cells[row, 5].Value),
                        Income = Convert.ToInt32(worksheet.Cells[row, 6].Value),
                        School = worksheet.Cells[row, 7].Value.ToString(),
                        Class = worksheet.Cells[row, 8].Value.ToString(),
                    });
                }
            }

            return new ObservableCollection<object>(students);
        }
        public static ObservableCollection<object> ReadTeachers(string filePath)
        {
            List<Teacher> teachers = new List<Teacher>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Teacher"];
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    teachers.Add(new Teacher
                    {
                        ID = worksheet.Cells[row, 1].Value.ToString(),
                        Name = worksheet.Cells[row, 2].Value.ToString(),
                        Age = Convert.ToInt32(worksheet.Cells[row, 3].Value),
                        Address = worksheet.Cells[row, 4].Value.ToString(),
                        TaxCode = Convert.ToInt32(worksheet.Cells[row, 5].Value),
                        Income = Convert.ToInt32(worksheet.Cells[row, 6].Value),
                        School = worksheet.Cells[row, 7].Value.ToString(),

                        // Assign other properties accordingly
                    });
                }
            }

            return new ObservableCollection<object>(teachers);
        }
        public static ObservableCollection<object> ReadEmployees(string filePath)
        {
            List<Employee> employees = new List<Employee>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Employees"];
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    employees.Add(new Employee
                    {
                        ID = worksheet.Cells[row, 1].Value.ToString(),
                        Name = worksheet.Cells[row, 2].Value.ToString(),
                        Age = Convert.ToInt32(worksheet.Cells[row, 3].Value),
                        Address = worksheet.Cells[row, 4].Value.ToString(),
                        TaxCode = Convert.ToInt32(worksheet.Cells[row, 5].Value),
                        Income = Convert.ToInt32(worksheet.Cells[row, 6].Value),
                        // Assign other properties accordingly
                    });
                }
            }

            return new ObservableCollection<object>(employees);
        }
        public ObservableCollection<ObservableCollection<string>> ReadExcel(string filePath)
        {
            ObservableCollection<ObservableCollection<string>> allData = new ObservableCollection<ObservableCollection<string>>();

            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    ObservableCollection<string> sheetData = new ObservableCollection<string>();
                    int rowCount = worksheet.Dimension.Rows;
                    int columnCount = worksheet.Dimension.Columns;

                    for (int row = 1; row <= rowCount; row++)
                    {
                        for (int col = 1; col <= columnCount; col++)
                        {
                            string cellValue = worksheet.Cells[row, col].Value?.ToString();
                            sheetData.Add(cellValue);
                        }
                    }

                    allData.Add(sheetData);
                }
            }

            return allData;
        }
        public static ObservableCollection<ObservableCollection<string>> ReadExcel(string filePath, string sheetName)
        {
            ObservableCollection<ObservableCollection<string>> sheetData = new ObservableCollection<ObservableCollection<string>>();

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);

                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[sheetName];

                    if (worksheet != null)
                    {
                        int rowCount = worksheet.Dimension.Rows;
                        int columnCount = worksheet.Dimension.Columns;

                        for (int row = 1; row <= rowCount; row++)
                        {
                            ObservableCollection<string> rowData = new ObservableCollection<string>();

                            for (int col = 1; col <= columnCount; col++)
                            {
                                rowData.Add(worksheet.Cells[row, col].Value?.ToString());
                            }

                            sheetData.Add(rowData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading Excel file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                // Xử lý ngoại lệ (ví dụ: ghi log, hiển thị thông báo lỗi)
                Console.WriteLine("Error reading Excel file: " + ex.Message);
            }

            return sheetData;
            
        }

        
    }
}
