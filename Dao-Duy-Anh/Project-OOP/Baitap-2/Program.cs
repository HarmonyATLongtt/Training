using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baitap_2
{   
    public class TaxData
    {
        public double GetTaxCoe(int age, double income)
        {
            double heSoThue =0; 
            if (age > 18)
            {
                if( income < 9000000)
                {
                    heSoThue = 0.05;
                }
                else if (income > 9000000 && income <= 15000000)
                {
                    heSoThue = 0.1;
                }
                else if (income > 15000000 && income <= 20000000)
                {
                    heSoThue = 0.15;
                }
                else if (income > 20000000 && income <= 30000000)
                {
                    heSoThue = 0.2;
                }
            } else
            {
               heSoThue = 0;
            }
            return heSoThue;
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Income { get; set; }
        public double TaxCoe { get; set; }

        public Person(int id, string name, int age, int income, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxData.GetTaxCoe(age,income);
        }

        public bool Equals(Person p)
        {
            if (p == null) return false;
            return Id == p.Id && Name == p.Name && Age == p.Age && TaxCoe == p.TaxCoe && Income == p.Income && TaxCoe == p.TaxCoe;
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }

    }

    internal class Program
    {
        static List<Person> listPeople = new List<Person>();
        static void Init()
        {
            TaxData taxData = new TaxData();
            listPeople.Add(new Person(1, "Dao Duy Anh", 22, 15000000, taxData));
            listPeople.Add(new Person(2, "Nguyen Quang Trung", 17, 1500000, taxData));
            listPeople.Add(new Person(3, "Vu Trong Nghia", 23, 3000000, taxData));
            listPeople.Add(new Person(4, "Dinh Ba Dat", 22, 25000000, taxData));
            listPeople.Add(new Person(5, "Tran The Long", 28, 30000000, taxData));
        }
        static void Output()
        {
            foreach (var person in listPeople)
            {
                Console.WriteLine($"Id: {person.Id}, Name: {person.Name}, Tax: {person.GetTax()}, Income: {person.Income}, TaxCoe: {person.TaxCoe}");
            }
            Console.ReadLine();
        }
        static void Main(string[] args)
        {
            Init();
            Output();
            Console.ReadKey();
        }
    }
}
