using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public double Income { get; set; }
    public double TaxCoe { get; set; }

    public bool Equals(Person p)
    {
        return this.Id == p.Id && this.Name == p.Name && this.Age == p.Age && this.Income == p.Income && this.TaxCoe == p.TaxCoe;
    }


    public double GetTax()
    {
        return Income * TaxCoe;
    }
}

public class Program
{
    public static List<Person> people = new List<Person>();

    public static void Init()
    {
        people.Add(new Person { Id = 11, Name = "Duc Anh ", Age = 26, Income = 50000, TaxCoe = 0.10 });
        people.Add(new Person { Id = 22, Name = "Kieu Anh", Age = 45, Income = 60000, TaxCoe = 0.15 });
        people.Add(new Person { Id = 33, Name = "Quang Huy", Age = 42, Income = 70000, TaxCoe = 0.20 });
    }

    public static void Output()
    {
        foreach (var person in people)
        {
            Console.WriteLine($"ID: {person.Id}, Name: {person.Name}, Tax: {person.GetTax()}");
        }
    }

    static void Main(string[] args)
    {
        Init();
        Output();
        Console.ReadKey();
    }
}


