using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3
{
    internal class Employee:IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public string Class { get; set; }
        public string School { get; set; }
        public Employee(int id, string name, int age, double income, string company, string jobTitle)
        {
            TaxData instance = new TaxData();
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = instance.GetTaxCoe(age, income);
            Company = company;
            JobTitle = jobTitle;
        }

        public string GetInfo()
        {
            return $"Id={Id}, Name={Name}, Age={Age}, Income={Income}, TaxCoe={TaxCoe}, Company={Company}, JobTitle={JobTitle}";
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }
    }
}
