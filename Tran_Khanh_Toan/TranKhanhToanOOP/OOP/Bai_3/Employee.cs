using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    internal class Employee : Person
    {
        public string Company { get; set; }
        public string JobTitle { get; set; }

        public Employee()
        {
            
        }

        public Employee(int Id, string Name, int Age, double InCome, double TaxCoe, string Company, string JobTitle)
            : base(Id, Name, Age, InCome, TaxCoe)
        {
            this.Company = Company;
            this.JobTitle = JobTitle;
        }

        public Employee(int Id, string Name, int Age, double InCome, TaxData taxData, string Company, string JobTitle)
            : base(Id, Name, Age, InCome, taxData)
        {
            this.Company = Company;
            this.JobTitle = JobTitle;
        }
        public new string GetInfo()
        {
            return $"Employee: {Id}_{Name}_{Age}_{Company}_{JobTitle}_{InCome}_{TaxCoe}";
        }
    }
}
