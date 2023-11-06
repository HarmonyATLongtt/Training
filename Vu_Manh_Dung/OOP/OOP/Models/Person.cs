using System;
using OOP.Data;

namespace OOP.Models
{
    public class Person : IPerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public TaxData Taxcoe = new TaxData();

        public Person()
        { }

        public Person(string id, string name, int age, double income)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;

        }
        public bool Equals(Person other)
        {
            return this.Id == other.Id;
        }
        public double GetTax()
        {
            return Income * Taxcoe.GetTaxCoe(Age, Income);
        }
        public virtual void Init()
        {
            Console.Write("Id: "); Id = Console.ReadLine();
            Console.Write("Name: "); Name = Console.ReadLine();
            Console.Write("Age: "); Age = int.Parse(Console.ReadLine());
            Console.Write("Income: "); Income = double.Parse(Console.ReadLine());
        }

        public static void Title()
        {
            Console.Write($"{"Id",5}{"Name",20}{"Age",10}");
        }
        public virtual void GetInfo()
        {
            Console.Write($"{Id,5}{Name,20}{Age,10}");
        }
    }
}