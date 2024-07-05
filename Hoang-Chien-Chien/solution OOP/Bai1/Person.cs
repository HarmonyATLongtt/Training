using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai1
{
    public class Person
    {
        private int id;
        private string name;
        private int age;
        private double income;
        private double taxcoe;
        public Person() { }
        public Person(int id, string name, int age, double income, double taxcoe)
        {
            this.Id = id;
            this.Name = name;
            this.Age = age;
            this.Income = income;
            this.Taxcoe = taxcoe;
        }

        public int Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }
        public int Age { get { return age; } set { age = value; } }
        public double Income { get { return income; } set { income = value; } }
        public double Taxcoe { get { return taxcoe; } set { taxcoe = value; } }
        public bool Equals(Person p)
        {
            if (p == null) return false;
            if (this.Id != p.Id || this.Name != p.Name || this.Age != p.Age || this.Income != p.Income || this.Taxcoe != p.Taxcoe) return false;
            return true;
        }
        public double GetTax()
        {
            return this.Taxcoe * this.Income;
        }

    }
}
