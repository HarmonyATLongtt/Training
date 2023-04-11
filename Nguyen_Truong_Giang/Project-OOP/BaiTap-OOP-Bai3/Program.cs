using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiTap_OOP_Bai3
{
    public class TaxData
    {
        public static double GetTaxCoe(int age, double income)
        {
            if (age < 0)
            {
                return 0;
            }
            else if (income <= 9000000)
            {
                return 0.05;
            }
            else if (income <= 15000000)
            {
                return 0.1;
            }
            else if (income <= 20000000)
            {
                return 0.15;
            }
            else if (income <= 30000000)
            {
                return 0.2;
            }
            else
            {
                return 0.25;
            }
        }
    }

    public interface IPerson
    {
        int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
        double Income { get; set; }
        double TaxCoe { get; set; }
        bool Equals(Person p);
        double GetTax();
        string GetInfo();
    }

    public class Person : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }

        public Person(int id, string name, int age, double income, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = TaxData.GetTaxCoe(age, income);
        }

        public bool Equals(Person p)
        {
            return Id == p.Id && Name == p.Name && Age == p.Age && Income == p.Income && TaxCoe == p.TaxCoe;
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }

        public string GetInfo()
        {
            return $"Id: {Id}, Name: {Name}, Age: {Age}, Income: {Income}, TaxCoe: {TaxCoe}";
        }
    }

    public class Student : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }
        public string School { get; set; }
        public string Class { get; set; }
        

        public Student(int id, string name, int age, string school, string className, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            //Income = income;
            School = school;
            Class = className;
            //TaxCoe = TaxData.GetTaxCoe(age, income);
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }

        public bool Equals(Person Student)
        {
            if (Student == null)
            {
                return false;
            }
            return Id == Student.Id && Name == Student.Name && Age == Student.Age && Income == Student.Income;
        }

        public string GetInfo()
        {
            return $"Student - ID: {Id} \n - Name: {Name} \n - Age: {Age} \n - School: {School} \n - Class: {Class} \n";
        }
    }

    public class Teacher : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }
        public string School { get; set; }

        public Teacher(int id, string name, int age, string school, double income, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            School = school;
            TaxCoe = TaxData.GetTaxCoe(age, income);
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }

        public bool Equals(Person Teacher)
        {
            if (Teacher == null)
            {
                return false;
            }
            return Id == Teacher.Id && Name == Teacher.Name && Age == Teacher.Age && Income == Teacher.Income;
        }

        public string GetInfo()
        {
            return $"Teacher - ID: {Id} \n - Name: {Name} \n - Age: {Age} \n - School: {School} \n - Income: {Income} \n - Tax: {GetTax()} \n";
        }
    }

    public class Employee : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }

        public Employee(int id, string name, int age, string company, string jobtitle, double income, TaxData taxData)
        {
            Id = id;    
            Name = name;
            Age = age;
            Income = income;
            Company = company;
            JobTitle = jobtitle;
            TaxCoe = TaxData.GetTaxCoe(age, income);
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }

        public bool Equals(Person Teacher)
        {
            if (Teacher == null)
            {
                return false;
            }
            return Id == Teacher.Id && Name == Teacher.Name && Age == Teacher.Age && Income == Teacher.Income;
        }

        public string GetInfo()
        {
            return $"Teacher - ID: {Id} \n - Name: {Name} \n - Age: {Age} \n - Company: {Company} \n - JobTitle: {JobTitle} \n - Income: {Income} \n - Tax: {GetTax()} \n";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TaxData taxData = new TaxData();

            List<Student> studentList = new List<Student>();
            studentList.Add(new Student(1, "Alice", 17, "School A", "Class 1", taxData));
            studentList.Add(new Student(2, "Bob", 19, "School B", "Class 2", taxData));
            studentList.Add(new Student(3, "Charlie", 16, "School C", "Class 3", taxData));

            List<Teacher> teacherList = new List<Teacher>();
            teacherList.Add(new Teacher(1, "Yasuo", 27, "School A", 15000000, taxData));
            teacherList.Add(new Teacher(2, "Zed", 29, "School B", 25000000, taxData));
            teacherList.Add(new Teacher(3, "Gangplank", 26, "School C", 9000000, taxData));
            teacherList.Add(new Teacher(4, "Fiora", 30, "School C", 30000000, taxData));

            List<Employee> employeeList = new List<Employee>();
            employeeList.Add(new Employee(1, "Jinx", 27, "Harmony A","Deverloper", 30000000, taxData));
            employeeList.Add(new Employee(2, "Giang", 29, "Harmony B", "Tester", 25000000, taxData));


            List<Person> people = new List<Person>();
            OutputStudent(studentList);
            Console.WriteLine("***************************** \n");
            OutputTeacher(teacherList);
            Console.WriteLine("***************************** \n");
            OutputEmployee(employeeList);

        }

        public static void OutputStudent(List<Student> studentList)
        {
            int studentCount = 0;

            foreach (var students in studentList)
            {
                studentCount++;
                var student = (Student)students;
                Console.WriteLine($"{student.GetInfo()}");
            }
            Console.WriteLine($"Total Students: {studentCount}\n");
            Console.ReadLine();
        }

        public static void OutputTeacher(List<Teacher> teacherList)
        {
            int teacherCount = 0;

            foreach (var teachers in teacherList)
            {
                teacherCount++;
                var teacher = (Teacher)teachers;
                Console.WriteLine($"{teacher.GetInfo()}");
            }
            Console.WriteLine($"Total Teacher: {teacherCount}\n");
            Console.ReadLine();
        }

        public static void OutputEmployee(List<Employee> employeeList)
        {
            int employeeCount = 0;

            foreach (var employees in employeeList)
            {
                employeeCount++;
                var employee = (Employee)employees;
                Console.WriteLine($"{employee.GetInfo()}");
            }
            Console.WriteLine($"Total Employees: {employeeCount}\n");
            Console.ReadLine();
        }
    }

    
}
