using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    public class Person
    {
        private string sID;
        private string sName;
        private int sAge;
        private double sIncome;
        private double dTaxCoe;

        public string ID
        {
            get { return sID; }
            set { sID = value; }
        }

        public string Name
        {
            get { return sName; }
            set { sName = value; }
        }

        public int Age
        {
            get { return sAge; }
            set { sAge = value; }
        }

        public double Income
        {
            get { return sIncome; }
            set { sIncome = value; }
        }

        public double TaxCoe
        {
            get { return dTaxCoe; }
            set { dTaxCoe = value; }
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
        ~Person()
        {

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
