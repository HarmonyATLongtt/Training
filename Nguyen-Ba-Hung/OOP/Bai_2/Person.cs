using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_2
{
    internal class Person
    {
        private string Id;
        private string Name;
        private int Age;
        private int Income;
        private double TaxCoe;

        public string id { get => Id; set => Id = value; }
        public string name { get => Name; set => Name = value; }
        public int age { get => Age; set => Age = value; }
        public int income { get => Income; set => Income = value; }
        public double taxCoe { get => TaxCoe; set => TaxCoe = value; }

        public Person()
        {
        }

        public Person(string Id, string Name, int Age, int Income, int TaxCoe)
        {
            id = Id;
            name = Name;
            age = Age;
            income = Income;
            taxCoe = TaxCoe;
        }

        public Person(string Id, string Name, int Age, int Income)
        {
            id = Id;
            name = Name;
            age = Age;
            income = Income;
            TaxData taxData = new TaxData();
            taxCoe = taxData.GetTaxCoe(Age, Income);
        }

        public bool Equals(Person p)
        {
            return p is Person person &&
                   id == person.id &&
                   name == person.name &&
                   age == person.age &&
                   income == person.income &&
                   taxCoe == person.taxCoe;
        }

        public double GetTax() 
        {
            return taxCoe * income;
        }
    }
}
