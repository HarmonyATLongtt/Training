using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Baitap_3
{
    interface IPerson
    {
        string GetInfo();
    }

    class Person : IPerson
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
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public bool Equals(Person p)
        {
            if (p == null) return false;
            return Id == p.Id && Name == p.Name && Age == p.Age && TaxCoe == p.TaxCoe && Income == p.Income && TaxCoe == p.TaxCoe;
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
    class TaxData
    {
        public double GetTaxCoe(int age, double income)
        {
            double heSoThue = 0;
            if (age > 18)
            {
                if (income < 9000000)
                {
                    heSoThue = 0.05;
                }
                else if (income > 9000000 && income <= 15000000)
                {
                    heSoThue = 0.1;
                }
                else if (income > 15000000 && income <= 20000000)
                {
                    heSoThue = 0.15;
                }
                else if (income > 20000000 && income <= 30000000)
                {
                    heSoThue = 0.2;
                }
            }
            else
            {
                heSoThue = 0;
            }
            return heSoThue;
        }
    }
    class Student : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }
        public string School { get; set; }
        public string Class { get; set; }


        public Student(int id, string name, int age, string school, string className)
        {
            Id = id;
            Name = name;
            Age = age;
            School = school;
            Class = className;
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }

        public bool Equals(Student s)
        {
            if (s == null)
            {
                return false;
            }
            return Id == s.Id && Name == s.Name && Age == s.Age;
        }

        public string GetInfo()
        {
            return $"Student - ID: {Id} \n - Name: {Name} \n - Age: {Age} \n - School: {School} \n - Class: {Class} \n";
        }
    }
    class Teacher : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }
        public string School { get; set; }

        public Teacher(int id, string name, int age, double income, TaxData taxData, string school)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxData.GetTaxCoe(age, income);
            School = school;
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }

        public bool Equals(Teacher teacher)
        {
            if (teacher == null)
            {
                return false;
            }
            return Id == teacher.Id && Name == teacher.Name && Age == teacher.Age && Income == teacher.Income;
        }

        public string GetInfo()
        {
            return $"Teacher - ID: {Id} \n - Name: {Name} \n - Age: {Age} \n - School: {School} \n - Income: {Income} \n - Tax: {GetTax()} \n";
        }
    }
    class Employee : IPerson
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
            TaxCoe = taxData.GetTaxCoe(age, income);
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
            return $"Employee - ID: {Id} \n - Name: {Name} \n - Age: {Age} \n - Company: {Company} \n - JobTitle: {JobTitle} \n - Income: {Income} \n - Tax: {GetTax()} \n";
        }
    }
    internal class Program
    {
        
        static void Main(string[] args)
        {
            TaxData taxData = new TaxData();

            List<Student> studentList = new List<Student>();
            studentList.Add(new Student(1, "My", 17, "School A", "Class 1"));
            studentList.Add(new Student(2, "Linh", 18, "School B", "Class 2"));

            List<Teacher> teacherList = new List<Teacher>();
            teacherList.Add(new Teacher(1, "Quang", 27, 15000000, taxData, "School A"));
            teacherList.Add(new Teacher(2, "Anh", 29, 25000000, taxData, "School B"));


            List<Employee> employeeList = new List<Employee>();
            employeeList.Add(new Employee(1, "Jinx", 27, "Harmony A", "Deverloper", 30000000, taxData));
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

            foreach (var students in studentList)
            {
                Console.WriteLine($"{students.GetInfo()}");
            }
            Console.WriteLine($"Total Students: {studentList.Count}\n");
            Console.ReadLine();
        }

        public static void OutputTeacher(List<Teacher> teacherList)
        {

            foreach (var teachers in teacherList)
            {
                Console.WriteLine($"{teachers.GetInfo()}");
            }
            Console.WriteLine($"Total Teacher: {teacherList.Count}\n");
            Console.ReadLine();
        }

        public static void OutputEmployee(List<Employee> employeeList)
        {

            foreach (var employees in employeeList)
            {
                Console.WriteLine($"{employees.GetInfo()}");
            }
            Console.WriteLine($"Total Employees: {employeeList.Count}\n");
            Console.ReadLine();
        }
    }
}
