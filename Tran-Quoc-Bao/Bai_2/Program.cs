using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TaxData
{
    public double GetTaxCoe(int age, double income)
    {
        if (age < 18)
        {
            return 0;
        }
        else
        {
            if (income <= 9000000)
                return 0.05;
            else if (income <= 15000000)
                return 0.1;
            else if (income <= 20000000)
                return 0.15;
            else if (income <= 30000000)
                return 0.2;
            else
                return 0.25; 
        }
    }
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public double Income { get; set; }
    public double TaxCoe { get; set; }

    public Person(int id, string name, int age, double income, TaxData taxData)
    {
        Id = id;
        Name = name;
        Age = age;
        Income = income;
        if (age < 18)
        {
            TaxCoe = 0;
        }
        else
        {
            TaxCoe = taxData.GetTaxCoe(age, income);
        }
    }

    public double GetTax()
    {

        if (TaxCoe == 0)
        {
            return 0;
        }
        return Income * TaxCoe;
    }
}
public class Program
{
    public static void Main(string[] args)
    {
        TaxData taxData = new TaxData();
        Person person1 = new Person(1, "Duc Anh", 15, 5000000, taxData); 
        Person person2 = new Person(2, "Bao", 20, 12000000, taxData);
        Person person3 = new Person(3, "Binh", 25, 25000000, taxData);

        Console.WriteLine($"ID: {person1.Id}, Name: {person1.Name}, Tax: {person1.GetTax()}");
        Console.WriteLine($"ID: {person2.Id}, Name: {person2.Name}, Tax: {person2.GetTax()}");
        Console.WriteLine($"ID: {person3.Id}, Name: {person3.Name}, Tax: {person3.GetTax()}");
        Console.ReadKey();
    }
}
