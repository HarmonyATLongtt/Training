using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3
{
    public class Teacher :IPerson
    {
        private int id;
        private string name;
        private int age;
        private double income;
        private double taxcoe;
        private string schoool;
        
        public Teacher() { }
        public Teacher(int id, string name, int age, double income, string school)
        {
            this.Id = id;
            this.Name = name;
            this.Age = age;
            this.Income = income;
            this.Taxcoe = TaxData.GetTaxCoe(age, income);
            this.Schoool = school;
        }

        public int Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }
        public int Age { get { return age; } set { age = value; } }
        public double Income { get { return income; } set { income = value; } }
        public double Taxcoe { get { return taxcoe; } set { taxcoe = value; } }
        public string Schoool { get { return schoool; } set { schoool = value; } }

        public string GetInfo()
        {
            return $"_{this.Id}_{this.Name}_{this.Age}_{this.Schoool}_{this.Income}_{GetTax()}";
        }

        public double GetTax()
        {
            return this.Taxcoe * this.Income;
        }
    }
}
