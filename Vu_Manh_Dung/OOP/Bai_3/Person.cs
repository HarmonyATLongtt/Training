using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Bai_3
{
    public class Person : IPerson
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public float Taxcoe { get; set; }

        public Person() { }

        public Person(string id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }
        public Person(string id, string name, int age, double income, TaxData taxcoe)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            Taxcoe = taxcoe.GetTaxCoe(age,income);
        }

        virtual public void Init()
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
            Console.WriteLine("\t\t==========PERSON LIST==========");
            Console.Write(String.Format($"{"ID",0}{"Name",20}{"Age",10}"));
        }



        virtual public void GetInfo()
        {
            Console.Write(String.Format($"{Id,0}{Name,20}{Age,10}")) ;
        }

    }
}
