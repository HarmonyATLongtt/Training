using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_2
{
    public class Person
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public float Taxcoe { get; set; }

        public Person() { }
        public Person(string id, string name, int age, double income, TaxData taxcoe)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            Taxcoe = taxcoe.GetTaxCoe(age,income);
        }
        public bool Equals(Person p)
        {
            if (this.Id == p.Id) return true;
            else return false;
        }
        public float GetTax()
        {
            return (float) Income * Taxcoe;
        }
        public void Init()
        {
            Console.Write("Person id: ");
            Id = Console.ReadLine();
            Console.Write("Person name: ");
            Name = Console.ReadLine();
            Console.Write("Person age: ");
            Age = int.Parse(Console.ReadLine());
            Console.Write("Incom: ");
            Income = double.Parse(Console.ReadLine());
            Console.Write("Taxcoe: ");
            Taxcoe = float.Parse(Console.ReadLine());
        }

        public static void Title()
        {
            Console.WriteLine("==========PERSON LIST==========");
            Console.WriteLine(String.Format($"{"ID",0}{"Name",20}{"Tax",10}"));
        }

        public void Output()
        {
            Console.WriteLine(String.Format($"{Id,0}{Name,20}{GetTax(),10}"));
        }
    }
}
