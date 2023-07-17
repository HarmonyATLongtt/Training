using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_2
{
    public class TaxData
    {
        public float GetTaxCoe(int age, float income)
        {
            return age < 18 ? 0 : income <= 9000000 ? 0.05f : income <= 15000000 ? 0.1f : income <= 20000000 ? 0.15f : 0.2f;
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }
        public float TaxCoe { get; set; }

        public Person() { }

        public Person(int id, string? name, int age, float income, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public bool Equals(Person other)
        {
            return Id == other.Id && Name == other.Name && Age == other.Age && TaxCoe == other.TaxCoe;
        }

        public float GetTax()
        {
            return Income * TaxCoe;
        }
    }

    public class Program
    {
        private List<Person>? li;
        public void Init()
        {
            TaxData taxData = new TaxData();
            li = new List<Person>
            {
                new Person(1, "Nguyen Van A", 19, 125000, taxData),
                new Person(2, "Nguyen Van B", 20, 100000, taxData),
                new Person(3, "Nguyen Van C", 18, 210000, taxData),
                new Person(4, "Nguyen Van D", 21, 150000, taxData),
                new Person(5, "Nguyen Van E", 19, 300000, taxData)

            };
        }

        public void Output()
        {
            foreach (var item in li)
            {
                Console.WriteLine("ID: {0}, Name: {1}, Tax: {2}", item.Id, item.Name, item.TaxCoe);
            }
        }

    }

    public class Bai_2
    {
        public static void Main(string[] args)
        {
            Program p = new Program();
            p.Init();
            p.Output();
        }
    }
}
