using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiTap_OOP_Bai3
{
    public class TaxData
    {
        public static double GetTaxCoe(int age, double income)
        {
            if (age < 0)
            {
                return 0;
            }
            else if (income <= 9000000)
            {
                return 0.05;
            }
            else if (income <= 15000000)
            {
                return 0.1;
            }
            else if (income <= 20000000)
            {
                return 0.15;
            }
            else if (income <= 30000000)
            {
                return 0.2;
            }
            else
            {
                return 0.25;
            }
        }
    }

    public interface IPerson
    {
        int Id { get; set; }
        string Name { get; set; }
        int Age { get; set; }
        double Income { get; set; }
        double TaxCoe { get; set; }
        bool Equals(Person p);
        double GetTax();
        string GetInfo();
    }

    public class Person : IPerson
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
            TaxCoe = TaxData.GetTaxCoe(age, income);
        }

        public bool Equals(Person p)
        {
            return Id == p.Id && Name == p.Name && Age == p.Age && Income == p.Income && TaxCoe == p.TaxCoe;
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }

        public string GetInfo()
        {
            return $"Id: {Id}, Name: {Name}, Age: {Age}, Income: {Income}, TaxCoe: {TaxCoe}";
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
