using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Employee:Person
    {
        public string Company { get; set; }
        public string JobTitle { get; set; }

        public Employee(string id, string name, int age, int income, TaxData taxcoe, string company, string jobtitle):base(id, name, age, income, taxcoe)
        {
            this.Company = company;
            this.JobTitle = jobtitle;
        }

        public override string GetInfo()
        {
            return base.GetInfo() +$"{Company}\t{JobTitle}\t{Income}\t{Taxcoe}";
        }
    }
}
