using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP.Models
{
    public class Employee : Person
    {
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public Employee() { }
        public Employee(string id, string name, int age, double income, string company, string jobTitle) : base(id, name, age, income)
        {
            Company = company;
            JobTitle = jobTitle;
        }
        public override void Init()
        {
            base.Init();
            Console.Write("Comapy: "); Company = Console.ReadLine();
            Console.Write("Job title: "); JobTitle = Console.ReadLine();
        }
        public static void Title()
        {
            Person.Title();
            Console.WriteLine($"{"Company",15}{"JobTitle",15}{"Income",15}{"Tax",15}");
        }
        public override void GetInfo()
        {
            base.GetInfo();
            Console.WriteLine($"{Company,15}{JobTitle,15}{Math.Round(Income, 2),15}{Math.Round(GetTax(),2),15}");
        }
    }
}
