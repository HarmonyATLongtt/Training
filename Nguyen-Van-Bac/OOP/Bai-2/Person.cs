using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_2
{
    
    public class Person
    {
        public Person()
        {
            
        }
        public Person(int id, string name, int age, double income, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }
        public bool Equals(Person p)
        {
            return this.Id == p.Id && this.Name == p.Name && this.Age == p.Age && this.Income == p.Income && this.TaxCoe == p.TaxCoe;
        }
        public double GetTax()
        {
            return this.TaxCoe * this.Income;
        }
    }
    
}
