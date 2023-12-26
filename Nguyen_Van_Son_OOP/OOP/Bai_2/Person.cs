using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_2
{
    public class Person
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }
        public float TaxCoe { get; set; }

        public Person() { }
        public Person(int id, string? name, int age, float income, float taxCoe)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxCoe;
        }
        public Person(int id, string? name, int age, float income, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }


        public bool Equal(Person p)
        {
            if (this.Id == p.Id && this.Name == p.Name && this.Age == p.Age && this.Income == p.Income && this.TaxCoe == p.TaxCoe)
                return true;
            return false;
        }

        public float GetTax()
        {
            return TaxCoe * Income;
        }
    }
}
