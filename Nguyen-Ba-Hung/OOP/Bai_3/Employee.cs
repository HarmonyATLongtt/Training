using Bai_2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    internal class Employee : IPerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Income { get; set; }
        public double TaxCoe { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }

        public Employee()
        {

        }

        public Employee(string id, string name, int age, int income, string company, string jobTitle)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxData taxData = new TaxData();
            TaxCoe = taxData.GetTaxCoe(age, income);
            Company = company;
            JobTitle = jobTitle;
        }

        public string GetInfo()
        {
            return "\t" + Id + "\t" + Name + "\t" + Age + "\t" + Company + "\t" + JobTitle + "\t" + Income + "\t" + (TaxCoe * Income);
        }
    }
}
