using Bai_2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    internal class Employee : Person, IPerson
    {
        public string Company;
        public string JobTitle;
        public Employee() { }

        public Employee(int id, string? name, int age, float income, float taxCoe, string company, string jobTitle)
            : base(id, name, age, income, taxCoe)
        {
            this.Company = company;
            this.JobTitle = jobTitle;
        }
        public Employee(int id, string? name, int age, float income, TaxData taxData, string company, string jobTitle)
            : base(id, name, age, income, taxData)
        {
            this.Company = company;
            this.JobTitle = jobTitle;
            this.TaxCoe = taxData.GetTaxCoe(age, income);
        }
        public static void Title()
        {
            Console.WriteLine($"{"Id",-5}{"Name",-15}{"Age",-5}{"Company",-15}{"JobTitle",-10}{"Income",-15}{"Tax"}");
        }
        public void GetInfo()
        {
            Console.WriteLine($"{this.Id,-5}{this.Name,-15}{this.Age,-5}{this.Company,-15}{this.JobTitle,-10}{this.Income,-15}{this.GetTax()}");
        }
    }
}
