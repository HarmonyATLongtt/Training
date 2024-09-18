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
        private string Id;
        private string name;
        private int age;
        private double income;
        private double taxCoe;

        public string ID
        {
            get { return Id; }
            set { Id = value; }
        }
        public string Name
        {
            set { name = value; }
            get { return name; }
        }
        public int Age
        {
            get { return Age; }
            set { age = value; }
        }
        public double Income
        {
            set { income = value; }
            get{ return income;}
        }
        public double TaxCoe
        {
            set{ taxCoe = value; }
            get { return taxCoe; }
        }
        public bool Equals(Person a)
        {
            return ID == a.ID && Name == a.Name && Age == a.Age && Income == a.Income && TaxCoe == a.TaxCoe;
        }
        public double GetTax()
        {
            return Income * TaxCoe;
        }
    }
}
