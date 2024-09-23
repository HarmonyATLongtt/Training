using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    public class Person
    {
        private string _id;
        private string _name;
        private int _age;
        private double _income;
        private double _taxCoe;

        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Age
        {
            get { return _age; }
            set { _age = value; }
        }

        public double Income
        {
            get { return _income; }
            set { _income = value; }
        }

        public double TaxCoe
        {
            get { return _taxCoe; }
            set { _taxCoe = value; }
        }

        public Person()
        {

        }

        public Person(string id, string name, int age, double income, double taxCoe)
        {
            ID = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxCoe;
        }
        

        public bool Equals(Person p)
        {
            if (p == null) return false;
            return ID == p.ID;
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }

        
    }
}
