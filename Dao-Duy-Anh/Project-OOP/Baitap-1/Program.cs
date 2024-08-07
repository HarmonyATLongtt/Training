using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baitap_1
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Income { get; set; }
        public double TaxCoe { get; set; }  

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

        static void Init ()
        {
            listPeople.Add(new Person { Id = 1, Name = "Dao Duy Anh", Age = 22, Income = 25000, TaxCoe = 0.1});
            listPeople.Add(new Person { Id = 2, Name = "Nguyen Truong Giang", Age = 28, Income = 35000, TaxCoe = 0.2 });
            listPeople.Add(new Person { Id = 3, Name = "Phung Tuan An", Age = 52, Income = 125000, TaxCoe = 0.3});
           
        }

        static void Output()
        {
            foreach (Person person in listPeople)
            {
                Console.WriteLine($"Id: {person.Id}, Name: {person.Name}, Tax: {person.GetTax()}");
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
