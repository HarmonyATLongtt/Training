using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{

    public class Person // Declare class Person and nescess attribute
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }
        public float TaxCoe { get; set; }

        public Person() { }

        public Person(int id, string? name, int age, float income, float taxCoe)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = taxCoe;
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
            li = new List<Person>();
            li.Add(new Person(1, "Nguyen Van A", 19, 125000, 0.3f));
            li.Add(new Person(2, "Nguyen Van B", 20, 100000, 0.1f));
            li.Add(new Person(3, "Nguyen Van C", 18, 210000, 0.4f));
            li.Add(new Person(4, "Nguyen Van D", 21, 150000, 0.2f));
            li.Add(new Person(5, "Nguyen Van E", 19, 300000, 0.5f));
        }

        public void Output()
        {
            foreach (var item in li)
            {
                Console.WriteLine("ID: {0}, Name: {1}, Tax: {2}", item.Id, item.Name, item.TaxCoe);
            }
        }
    }

    public class Bai_1
    {
        public static void Main(string[] args)
        {
            Program p = new Program();
            p.Init();
            p.Output();
        }
    }
}
