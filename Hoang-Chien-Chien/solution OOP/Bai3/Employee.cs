using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3
{
    public class Employee : IPerson
    {
        private int id;
        private string name;
        private int age;
        private double income;
        private double taxcoe;
        private string company;
        private string jobTitle;
        
        public Employee() { }
        public Employee(int id, string name, int age, double income, string company, string jobTitle)
        {
            this.Id = id;
            this.Name = name;
            this.Age = age;
            this.Income = income;
            this.Taxcoe = TaxData.GetTaxCoe(age, income);
            this.Company = company;
            this.JobTitle = jobTitle;
        }

        public int Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }
        public int Age { get { return age; } set { age = value; } }
        public double Income { get { return income; } set { income = value; } }
        public double Taxcoe { get { return taxcoe; } set { taxcoe = value; } }
        public string Company { get { return company; } set { company = value; } }
        public string JobTitle { get { return jobTitle; } set { jobTitle = value; } }

        public string GetInfo()
        {
            return $"_{this.Id}_{this.Name}_{this.Age}_{this.Company}_{this.JobTitle}_{this.Income}_{GetTax()}";
        }

        public double GetTax()
        {
            return this.Taxcoe * this.Income;
        }
    }
}
