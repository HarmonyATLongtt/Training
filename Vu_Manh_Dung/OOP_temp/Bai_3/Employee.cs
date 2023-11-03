using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Employee:Person
    {
        public string? Company { get; set; }
        public string? JobTitle { get; set; }

        public Employee() { }

        public Employee(string id, string name, int age, int income, TaxData taxcoe, string company, string jobtitle):base(id, name, age, income, taxcoe)
        {
            this.Company = company;
            this.JobTitle = jobtitle;
        }

        public override void Init()
        {
            base.Init();
            Console.Write("Company: ");
            Company = Console.ReadLine();
            Console.Write("Job Title: ");
            JobTitle = Console.ReadLine();
        }

        public static void Title()
        {
            Person.Title();
            Console.WriteLine(String.Format($"{"Company",20}{"Job title",20}{"Income",20}{"Tax",20}"));
        }

        public override void GetInfo()
        {
            base.GetInfo();
            Console.WriteLine(String.Format($"{Company,20}{JobTitle,20}{Income,20}{Taxcoe,20}"));
        }
    }
}
