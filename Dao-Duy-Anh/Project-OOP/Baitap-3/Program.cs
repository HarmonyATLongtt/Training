using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Baitap_3
{
    interface IPerson
    {
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
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public virtual bool Equals(Person p)
        {
            if (p == null) return false;
            return Id == p.Id && Name == p.Name && Age == p.Age && TaxCoe == p.TaxCoe && Income == p.Income && TaxCoe == p.TaxCoe;
        }

        public virtual double GetTax()
        {
            return Income * TaxCoe;
        }

        public virtual string GetInfo()
        {
            return $"Id: {Id}, Name: {Name}, Age: {Age}, Income: {Income}, TaxCoe: {TaxCoe}";
        }
    }
    public class TaxData
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
    public class Student : Person
    {
        //public new int Id { get; set; }
        //public new string Name { get; set; }
        //public new int Age { get; set; }
        //public new double Income { get; set; }
        //public new double TaxCoe { get; set; }
        public string School { get; set; }
        public string Class { get; set; }


        public Student(int id, string name, int age, double income, TaxData taxData, string school, string className) 
            : base ( id,  name,  age,  income, taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxData.GetTaxCoe(age, income);
            School = school;
            Class = className;
        }

        public override double GetTax()
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

        public override string GetInfo()
        {
            return $"Student - ID: {Id} \n - Name: {Name} \n - Age: {Age} \n - School: {School} \n - Class: {Class} \n";
        }
    }
    public class Teacher : Person
    {
        //public int Id { get; set; }
        //public string Name { get; set; }
        //public int Age { get; set; }
        //public double Income { get; set; }
        //public double TaxCoe { get; set; }
        public string School { get; set; }

        public Teacher(int id, string name, int age, double income, TaxData taxData, string school)
            : base(id, name, age, income, taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxData.GetTaxCoe(age, income);
            School = school;
        }

        public override double GetTax()
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

        public override string GetInfo()
        {
            return $"Teacher - ID: {Id} \n - Name: {Name} \n - Age: {Age} \n - School: {School} \n - Income: {Income} \n - Tax: {GetTax()} \n";
        }
    }
    public class Employee : Person
    {
        //public int Id { get; set; }
        //public string Name { get; set; }
        //public int Age { get; set; }
        //public double Income { get; set; }
        //public double TaxCoe { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }

        public Employee(int id, string name, int age, string company, string jobtitle, double income, TaxData taxData)
            : base(id, name, age, income, taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            Company = company;
            JobTitle = jobtitle;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public override double GetTax()
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

        public override string GetInfo()
        {
            return $"Employee - ID: {Id} \n - Name: {Name} \n - Age: {Age} \n - Company: {Company} \n - JobTitle: {JobTitle} \n - Income: {Income} \n - Tax: {GetTax()} \n";
        }
    }
    internal class Program
    {
        static TaxData taxData = new TaxData();
        static List<Person> listPeople = new List<Person>();
        static void Output()
        {
            Console.WriteLine("Total Student:" +listPeople.Count(p=>p is Student));
            foreach (Person person in listPeople.Where(p=>p is Student))
            {
                Console.WriteLine(person.GetInfo());
            }
            Console.WriteLine("Total Teacher:" + listPeople.Count(p => p is Teacher));
            foreach (Person person in listPeople.Where(p => p is Teacher))
            {
                Console.WriteLine(person.GetInfo());
            }
            Console.WriteLine("Total Employee:" + listPeople.Count(p => p is Employee));
            foreach (Person person in listPeople.Where(p => p is Employee))
            {
                Console.WriteLine(person.GetInfo());
            }
        }

        static void Init()
        {
            //List<Student> studentList = new List<Student>();
            listPeople.Add(new Student(1, "My", 17, 0, taxData, "School A", "Class 1"));
            listPeople.Add(new Student(2, "Linh", 18, 0, taxData, "School B", "Class 2"));
            listPeople.Add(new Student(2, "Duy", 18, 0, taxData, "School B", "Class 2"));

            //List<Teacher> teacherList = new List<Teacher>();
            listPeople.Add(new Teacher(1, "Quang", 27, 15000000, taxData, "School A"));
            listPeople.Add(new Teacher(2, "Anh", 29, 25000000, taxData, "School B"));


            //List<Employee> employeeList = new List<Employee>();
            listPeople.Add(new Employee(1, "Dat", 27, "Harmony A", "Deverloper", 30000000, taxData));
            listPeople.Add(new Employee(2, "Giang", 29, "Harmony B", "Tester", 25000000, taxData));
        }

        static void Main(string[] args)
        {
            Init();
            Output();
            Console.ReadKey();
        }
    }
}
