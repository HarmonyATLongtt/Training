using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_2
{
    public class Person
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Income { get; set; }
        public float Taxcoe { get; set; }
        public Person(string id, string name, int age, int income, TaxData taxcoe)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            Taxcoe = taxcoe.GetTaxCoe(age,income);
        }
        public bool Equals(Person p)
        {
            if (this.Id == p.Id) return true;
            else return false;
        }
        public float GetTax()
        {
            return Income * Taxcoe;
        }

    }
}
