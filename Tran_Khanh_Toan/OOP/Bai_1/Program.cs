using Bai_1;
using System;

namespace Bai_1 // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        public static List<Person> Init()
        {
            return new List<Person>
                {
                new Person { Id = 1, Name = "Tran Khanh Toan", Age = 21, InCome = 10000, TaxCoe = 0.2 },
                new Person { Id = 2, Name = "Nguyen Van A", Age = 16, InCome = 13000, TaxCoe = 0.12 },
                new Person { Id = 3, Name = "Nguyen Van B", Age = 30, InCome = 17000, TaxCoe = 0.24 },
                new Person { Id = 4, Name = "Tran Khanh Toan1", Age = 22, InCome = 20000, TaxCoe = 0.3 }
                };
        }

        public static void Output(List<Person> people)
        {
            foreach (Person person in people)
            {
                Console.WriteLine("This ID is " + person.Id + " has name " + person.Name + " and " + person.Age + " and tax: " + person.GetTax());
            }
        }
        static void Main(string[] args)
        {
            List<Person> people = Init();
            Output(people);
        }

    }
}