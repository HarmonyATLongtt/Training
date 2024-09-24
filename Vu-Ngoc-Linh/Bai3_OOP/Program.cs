using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3_OOP
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Number of people: ");
            int t = int.Parse(Console.ReadLine());
            for(int i=0; i <t; i++)
            {
                Input();
            }
            Output();
            Console.ReadKey();
        }
        static List<Student> studentList = new List<Student>();
        static List<Employee> employeeList = new List<Employee>();
        static List<Teacher> teacherList = new List<Teacher>();
        static void Input()
        {
            Console.Write("Enter 1 for student, 2 for employee, 3 for teacher: ");
            int mark=int.Parse(Console.ReadLine());
            if (mark == 1)
            {
                Student st1 = new Student();
                st1.Init();
                studentList.Add(st1);
            }
            else if (mark == 2)
            {
                Employee emp1 = new Employee();
                emp1.Init();
                employeeList.Add(emp1);
            }
            else
            {
                Teacher teacher = new Teacher();
                teacher.Init();
                teacherList.Add(teacher);
            }
        }
        static void Output()
        {
            if (studentList.Count > 0)
            {
                Console.WriteLine("Student: {0}", studentList.Count);
                foreach (var st in studentList)
                {
                    Console.WriteLine(st.GetInfo());
                }
            }
            if (teacherList.Count > 0)
            {
                Console.WriteLine("Teacher: ", teacherList.Count);
                foreach (var emp in teacherList)
                {
                    Console.WriteLine(emp.GetInfo());
                }
            }
            if (employeeList.Count > 0)
            {
                Console.WriteLine("Employee: ", employeeList.Count);
                foreach (var emp in employeeList)
                {
                    Console.WriteLine(emp.GetInfo());
                }
            }
        }
    }
}
