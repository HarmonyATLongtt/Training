using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using WPFAPP.Model;

namespace WPFAPP.Extension.Excel
{
    public  class ExcelReader
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
                        Imcome = Convert.ToInt32(worksheet.Cells[row, 6].Value),
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
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Student"];
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    teachers.Add(new Teacher
                    {
                        ID = worksheet.Cells[row, 1].Value.ToString(),
                        Name = worksheet.Cells[row, 2].Value.ToString(),
                        Age = Convert.ToInt32(worksheet.Cells[row, 3].Value),
                        Address = worksheet.Cells[row, 4].Value.ToString(),
                        TaxCode = Convert.ToInt32(worksheet.Cells[row, 5].Value),
                        Imcome = Convert.ToInt32(worksheet.Cells[row, 6].Value),
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
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Student"];
                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    employees.Add(new Employee
                    {
                        ID = worksheet.Cells[row, 1].Value.ToString(),
                        Name = worksheet.Cells[row, 2].Value.ToString(),
                        Age = Convert.ToInt32(worksheet.Cells[row, 3].Value),
                        Address = worksheet.Cells[row, 4].Value.ToString(),
                        TaxCode = Convert.ToInt32(worksheet.Cells[row, 5].Value),
                        Imcome = Convert.ToInt32(worksheet.Cells[row, 6].Value),
                        // Assign other properties accordingly
                    });
                }
            }

            return new ObservableCollection<object>(employees);
        }

    }
}
