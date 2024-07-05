using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3
{
    internal class Program
    {
        static List<Employee> lstEmp = new List<Employee>();
        static List<Teacher> lstTe = new List<Teacher>();
        static List<Student> lstSt = new List<Student>();
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "infor3.txt");

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {                       
                        string[] data = line.Trim().Split(new char[] { ';' });
                        if(data[0].Trim().ToUpper().Contains("EMPLOYEE"))
                        {
                            Employee employee = new Employee();
                            employee.Id = Convert.ToInt32(data[1].Trim());
                            employee.Name = data[2].Trim();
                            employee.Age = Convert.ToInt32(data[3].Trim());
                            employee.Income = Convert.ToDouble(data[4].Trim());
                            employee.Taxcoe = Convert.ToDouble(TaxData.GetTaxCoe(employee.Age, employee.Taxcoe));
                            employee.Company = data[5].Trim();
                            employee.JobTitle = data[6].Trim();
                            lstEmp.Add(employee);
                            continue;
                        }

                        if (data[0].Trim().ToUpper().Contains("TEACHER"))
                        {
                            Teacher teacher = new Teacher();
                            teacher.Id = Convert.ToInt32(data[1].Trim());
                            teacher.Name = data[2].Trim();
                            teacher.Age = Convert.ToInt32(data[3].Trim());
                            teacher.Income = Convert.ToDouble(data[4].Trim());
                            teacher.Taxcoe = Convert.ToDouble(TaxData.GetTaxCoe(teacher.Age, teacher.Taxcoe));
                            teacher.Schoool = data[5].Trim();
                            lstTe.Add(teacher);
                            continue;
                        }

                        if (data[0].Trim().ToUpper().Contains("STUDENT"))
                        {
                            Student student = new Student();
                            student.Id = Convert.ToInt32(data[1].Trim());
                            student.Name = data[2].Trim();
                            student.Age = Convert.ToInt32(data[3].Trim());                           
                            student.Schoool = data[4].Trim();
                            student.ClassName = data[5].Trim();
                            lstSt.Add(student);
                            continue;
                        }
                    }
                    catch (Exception ex) { }

                }
            }

            Output();
            Console.ReadKey();
        }

       
        public static void Output()
        {
            Console.WriteLine("Student: "+ lstSt.Count());
            foreach(Student student in lstSt)
            {
                Console.WriteLine(student.GetInfo());
            }
            Console.WriteLine();
            Console.WriteLine("Teacher: " + lstTe.Count());
            foreach (Teacher teacher in lstTe)
            {
                Console.WriteLine(teacher.GetInfo());
            }
            Console.WriteLine();
            Console.WriteLine("Employee: " + lstEmp.Count());
            foreach (Employee employee in lstEmp)
            {
                Console.WriteLine(employee.GetInfo());
            }
        }
    }
}
