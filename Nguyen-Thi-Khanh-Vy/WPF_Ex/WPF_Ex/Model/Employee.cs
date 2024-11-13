using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Ex.Model
{
    public class Employee : Person
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }
        public string JobTitle { get; set; }
        public string Company { get; set; }
        public Employee() : base() { }

        public Employee(string iD, string name, int age, double income, double taxcoe,  string jobTitle, string company) : base(iD, name, age)
        {
            Income = income;
            TaxCoe = taxcoe;
            Company = company;
            JobTitle = jobTitle;
           
        }
        public double GetTax()
        {
            return Income * TaxCoe;
        }
        
    }
}
