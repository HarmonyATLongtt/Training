using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP.Models
{
    public class Teacher : Person
    {
        public string School {get; set;}
        public Teacher() { }
        public Teacher(string id, string name, int age, double income, string school) : base(id, name, age, income)
        {
            School = school;
        }
        public override void Init()
        {
            base.Init();
            Console.Write("School: "); School = Console.ReadLine();
        }
        public static void Title(){
            Person.Title();
            Console.WriteLine($"{"School",10}{"Income",20}{"Tax",20}");
        }
        public override void GetInfo()
        {
            base.GetInfo();
            Console.WriteLine($"{School,10}{Math.Round(Income,2),20}{Math.Round(GetTax(),2),20}");
        }
    }
}
