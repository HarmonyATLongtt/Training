using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class TaxData
    {
        public double GetTaxCoe(int age, double In)
        {
            if (age < 18)
            {
                return 0;
            }
            else
            {
                if (In <= 9000000)
                {
                    return 0.05;
                }
                else if (In <= 15000000)
                {
                    return 0.10;
                }
                else if (In <= 20000000)
                {
                    return 0.15;
                }
                else if (In <= 30000000)
                {
                    return 0.20;
                }
                else
                {
                    return 0.25;
                }
            }
        }
    }

    internal interface IPerson
    {
        int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
        double InCome { get; set; }
        double TaxCoe { get; set; }

        string GetInfo();
    }

    public class Student : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double InCome { get; set; }
        public double TaxCoe { get; set; }
        public string Class { get; set; }
        public string School { get; set; }

        public Student(int id, string name, int age, double income, TaxData tax, string className, string school)
        {
            this.Id = id;
            this.Name = name;
            this.Age = age;
            this.InCome = income;
            this.TaxCoe = tax.GetTaxCoe(age, income);
            this.Class = className;
            this.School = school;
        }

        public string GetInfo()
        {
            return $"Id: {Id}, Name: {Name}, Age: {Age}, Income: {InCome}, Tax Coefficient: {TaxCoe}, Class: {Class}, School: {School}";
        }
    }

    public class Teacher : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double InCome { get; set; }
        public double TaxCoe { get; set; }

        public string School { get; set; }

        public Teacher(int id, string name, int age, double income, TaxData tax, string school)
        {
            this.Id = id;
            this.Name = name;
            this.Age = age;
            this.InCome = income;
            this.TaxCoe = tax.GetTaxCoe(age, income);
            this.School = school;
        }

        public string GetInfo()
        {
            return $"Id: {Id}, Name: {Name}, Age: {Age}, Income: {InCome}, Tax Coefficient: {TaxCoe}, School: {School}";
        }
    }

    public class Employee : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double InCome { get; set; }
        public double TaxCoe { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }

        public Employee(int id, string name, int age, double income, TaxData tax, string cmy, string job)
        {
            this.Id = id;
            this.Name = name;
            this.Age = age;
            this.InCome = income;
            this.TaxCoe = tax.GetTaxCoe(age, income);
            this.Company = cmy;
            this.JobTitle = job;
        }

        public string GetInfo()
        {
            return $"Id: {Id}, Name: {Name}, Age: {Age}, Income: {InCome}, Tax Coefficient: {TaxCoe}, Company: {Company}, JObTitle: {JobTitle}";
        }
    }

    internal class Program
    {
        public static void OutPut(Student st)
        {
            Console.WriteLine(st.GetInfo());
        }

        public static void OutPutT(Teacher t)
        {
            Console.WriteLine(t.GetInfo());
        }

        public static void OutPutE(Employee st)
        {
            Console.WriteLine(st.GetInfo());
        }

        private static void Main(string[] args)
        {
            TaxData tax = new TaxData();
            List<Student> ls = new List<Student> {
                new Student(1, "Nam", 15, 2000, tax, "10A", "High School 1"),
                new Student(2, "Thu", 16, 2800, tax, "11B", "High School 2")
            };
            List<Teacher> te = new List<Teacher>
            {
                new Teacher(3,"Huyen",25,5000,tax,"Ngo Quyen High School"),
                new Teacher(4,"Nhan",28,6000,tax,"Ngo Quyen High School"),
                new Teacher(5,"TRi",22,3000,tax,"Ngo Quyen High School"),
            };
            List<Employee> e = new List<Employee> {
                new Employee(6, "vui", 22, 20000, tax, "nhan vien", "Bia Habeco"),
                new Employee(7, "ve", 32, 20800, tax, "nhan vien", "SamSung")
            };
            foreach (var item in ls)
            {
                OutPut(item);
            }
            Console.WriteLine("");
            foreach (var item in te)
            {
                OutPutT(item);
            }
            Console.WriteLine("");
            foreach (var item in e)
            {
                OutPutE(item);
            }

            Console.ReadKey();
        }
    }
}