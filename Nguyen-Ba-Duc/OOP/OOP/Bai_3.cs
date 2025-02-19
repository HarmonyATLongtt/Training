using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using OOP;

namespace OOP
{
    public interface IPerson

    {
        public void GetInfo();

        public bool Equals(Person p1, Person p2);

        public double GetTax();
    }

    public class Example : IPerson
    {
        public bool Equals(Person p1, Person p2)
        {
            throw new NotImplementedException();
        }

        public void GetInfo()
        {
            throw new NotImplementedException();
        }

        public double GetTax()
        {
            throw new NotImplementedException();
        }
    }

    public class Student : Person, IPerson
    {
        public string Class { get; set; }
        public string School { get; set; }

        public Student(string a, string b, int id, string name, int age)
        {
            this.Class = a;
            this.School = b;
            this.Id = id;
            this.Name = name;
            this.Age = age;
        }
    }

    public class Teacher : Person, IPerson
    {
        public string School;

        public Teacher(string school, int id, string name, int age, float income)
        {
            TaxData taxData = new TaxData();
            this.School = school;
            this.Id = id;
            this.Name = name;
            this.Age = age;
            this.Income = income;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }
    }

    public class Employee : Person, IPerson
    {
        public string Company;
        public string Jobtitle;

        public Employee(string a, string b, int id, string name, int age, float income)
        {
            TaxData taxData = new TaxData();
            this.Company = a;
            this.Jobtitle = b;
            this.Id = id;
            this.Name = name;
            this.Age = age;
            this.Income = income;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }
    }
}