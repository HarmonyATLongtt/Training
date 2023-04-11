using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiTap_OOP_Bai1
{
    class Person
    {
        public int id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }

        public bool Equals(Person p)
        {
            if (p == null) return false;
            return id == p.id && Name == p.Name && Age == p.Age && Income == p.Income && TaxCoe == p.TaxCoe;
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }
    }

    class Program
    {
        static List<Person> peopleList = new List<Person>();

        static void Main(string[] args)
        {
            Init();
            Output();
        }
        
        static void Init()
        {
            peopleList.Add(new Person { id = 1, Name = "Nguyen Truong Giang", Age = 22, Income = 5000000, TaxCoe = 0.1 });
            peopleList.Add(new Person { id = 2, Name = "Nguyen Linh Giang", Age = 23, Income = 3000000, TaxCoe = 0.2 });
            peopleList.Add(new Person { id = 3, Name = "Nguyen Trung", Age = 22, Income = 4500000, TaxCoe = 0.2 });
            peopleList.Add(new Person { id = 4, Name = "Tran The Long", Age = 28, Income = 10000000, TaxCoe = 0.4 });
        }

        static void Output()
        {
            foreach (var person in peopleList)
            {
                Console.WriteLine($"Id: {person.id}, Name: {person.Name}, Tax: {person.GetTax()}");
            }
            Console.ReadLine();
        }
    }
}
