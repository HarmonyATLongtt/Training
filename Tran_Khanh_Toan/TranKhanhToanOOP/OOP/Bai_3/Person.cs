using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Person : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double InCome { get; set; }
        public double TaxCoe { get; set; }

        public Person()
        {

        }

        public Person(int id, string name, int age, double inCome, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            InCome = inCome;
            TaxCoe = taxData.GetTaxData(this.Age, this.InCome);
        }
        public Person(int id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }

        public Person(int id, string name, int age, double inCome, double taxCoe)
        {
            Id = id;
            Name = name;
            Age = age;
            InCome = inCome;
            TaxCoe = taxCoe;
        }

        public bool Equals(Person p)
        {
            return this.Id == p.Id &&
                   this.Name == p.Name &&
                   this.Age == p.Age &&
                   this.InCome == p.InCome &&
                   this.TaxCoe == p.TaxCoe;
        }
        public double GetTax()
        {
            return (double)this.TaxCoe * (double)this.InCome;
        }

        public string GetInfo()
        {
            return ($"ID: {Id}, Name: {Name}, Age: {Age}, InCome: {InCome}, TaxCoe: {TaxCoe}");
        }
    }
}
