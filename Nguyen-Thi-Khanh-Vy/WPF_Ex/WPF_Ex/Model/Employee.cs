using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Ex.Model
{
    public class Employee : Person
    {
        public double Income { get; set; }
        public double TaxCoe { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        private TaxData taxData;

        public Employee() : base() { }

        public Employee(string iD, string name, int age, string company, string jobTitle, double income) : base(iD, name, age)
        {

        }
        public Employee(string iD, string name, int age, string company, string jobTitle, double income, TaxData taxData)
        {
            ID = iD;
            Name = name;
            Age = age;
            Company = company;
            JobTitle = jobTitle;
            Income = income;
            this.taxData = taxData;
            TaxCoe = taxData.GetTaxCoe(age, income);

        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }
        
    }
}
