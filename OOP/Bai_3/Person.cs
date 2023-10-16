using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bai_3
{
    public interface IPerson 
    {
        public string GetInfo();
    }
    public class Person:IPerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Income { get; set; }
        public float Taxcoe { get; set; }

        public Person(string id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }
        public Person(string id, string name, int age, int income, TaxData taxcoe)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            Taxcoe = taxcoe.GetTaxCoe(age,income);
        }

        virtual public string GetInfo()
        {
            return $"{Id}\t{Name}\t{Age}\t";
        }

    }
}
