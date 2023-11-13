using Microsoft.Extensions.Primitives;
using OfficeOpenXml;
using Bai_3;
using System;
using System.Collections;
using System.Xml;

class Program
{
    static ArrayList personlist = new ArrayList() { };

    static int student_num = 0;
    static int teacher_num = 0;
    static int employee_num = 0;
    static void Init()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        FileInfo existingFile = new FileInfo(@"F:\Coding\c#\Excel_data\Bai3.xlsx");
        using (ExcelPackage package = new ExcelPackage(existingFile))
        {

            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            var check_student = from i in worksheet.Cells["E2:E10"]
                           where i.Value.ToString() == "Student"
                           select i;
            foreach ( var cell in check_student)
            {
                TaxData mytax = new TaxData();

                int row= cell.Start.Row;
                string id = worksheet.Cells[row, 1].Value.ToString().Trim();
                string name = worksheet.Cells[row, 2].Value.ToString().Trim();
                string age = worksheet.Cells[row, 3].Value.ToString().Trim();
                string income = worksheet.Cells[row, 4].Value.ToString().Trim();
                string[] workplace = worksheet.Cells[row, 6].Value.ToString().Trim().Split("-");
                string st_class = workplace[0];
                string st_school = workplace[1];
                bool check = true;
                Student student = new Student(id, name, age, income, st_class, st_school, mytax);
                foreach (var i in personlist)
                {
                    if (i is Student)
                    {
                        var student1 = (Student)i;
                        if (student1.Equals(student) == false)
                        {
                            check = false;
                            break;
                        }
                    }
                }
                
                if (check == false){
                    break;
                }
                personlist.Add(student);
                student_num += 1;
            }
            var check_teacher = from i in worksheet.Cells["E2:E10"]
                                where i.Value.ToString() == "Teacher"
                                select i;
            foreach (var cell in check_teacher)
            {
                TaxData mytax = new TaxData();
                
                int row = cell.Start.Row;
                string id = worksheet.Cells[row, 1].Value.ToString().Trim();
                string name = worksheet.Cells[row, 2].Value.ToString().Trim();
                string age = worksheet.Cells[row, 3].Value.ToString().Trim();
                string income = worksheet.Cells[row, 4].Value.ToString().Trim();
                string st_school = worksheet.Cells[row, 6].Value.ToString().Trim();
                
                Teacher teacher = new Teacher(id, name, age, income, st_school, mytax);
                bool check = true;
                foreach (var i in personlist)
                {
                    if (i is Teacher)
                    {
                        var teacher1 = (Teacher)i;
                        if (teacher1.Equals(teacher) == false)
                        {
                            check = false;
                            break;
                        }
                    }
                }

                if (check == false)
                {
                    break;
                }
                personlist.Add(teacher);
                teacher_num += 1;
            }

            var check_employee = from i in worksheet.Cells["E2:E10"]
                                where i.Value.ToString() != "Teacher" && i.Value.ToString() != "Student"
                                 select i;
            foreach (var cell in check_employee)
            {
                TaxData mytax = new TaxData();
                
                int row = cell.Start.Row;
                string id = worksheet.Cells[row, 1].Value.ToString().Trim();
                string name = worksheet.Cells[row, 2].Value.ToString().Trim();
                string age = worksheet.Cells[row, 3].Value.ToString().Trim();
                string income = worksheet.Cells[row, 4].Value.ToString().Trim();
                string jobtitle = worksheet.Cells[row,5].Value.ToString().Trim();
                string workplace = worksheet.Cells[row, 6].Value.ToString().Trim();
               
                Employee employee = new Employee(id, name, age, income,jobtitle, workplace, mytax);
                bool check = true;
                foreach (var i in personlist)
                {
                    if (i is Employee)
                    {
                        var employee1 = (Employee)i;
                        if (employee1.Equals(employee) == false)
                        {
                            check = false;
                            break;
                        }
                    }
                }

                if (check == false)
                {
                    break;
                }
                personlist.Add(employee);
                employee_num += 1;
            }


        }
    }
    static void Output()
    {
        Console.WriteLine("Student: " + student_num);
        foreach(var i in personlist)
        {
            if (i is Student)
            {
                var student = (Student)i;
                student.GetInfo();
            }
        }
        Console.WriteLine("Teacher: " + teacher_num);
        foreach (var i in personlist)
        {
            if (i is Teacher)
            {
                var teacher = (Teacher)i;
                teacher.GetInfo();
            }
           
        }
        Console.WriteLine("Employee: " + employee_num);
        foreach (var i in personlist)
        {
            if (i is Employee)
            {
                var employee = (Employee)i;
                employee.GetInfo();
            }
        }
    }
    static void Main(string[] args)
    {
        Init();
        Output();

    }

}

