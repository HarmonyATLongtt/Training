using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Bai1_OOP
{
    public class Person
    {
        private string _id;
        public string ID { get; set; }

        private string _name;
        public string Name{ get; set;}

        private int _age;
        public int Age { get; set; }

        private double _income;
        public double Income { get; set; }

        private double _taxCoe;
        public double TaxCoe {  get; set; }

        public bool Equals(Person a)
        {
            return ID == a.ID && Name == a.Name && Age == a.Age && Income == a.Income && TaxCoe == a.TaxCoe;
        }
        public double GetTax()
        {
            return Income * TaxCoe;
        }
        public Person(string id, string name, int age, double income, double taxcoe)
        {
            ID = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxcoe;
        }
    }
}
