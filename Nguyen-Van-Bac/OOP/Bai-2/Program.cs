using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Bai_2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Person> people = Init();
            Output(people);
            
        }
        static void Output(List<Person> people)
        {
            foreach (var item in people)
            {
                Console.WriteLine($"ID: {item.Id}, Name: {item.Name}, Tax: {item.TaxCoe}");
            }
        }
        static List<Person> Init()
        {
            var ListPerson = new List<Person>()
            {
                new Person()
                {
                    Id = 1,
                    Name = "Nam",
                    Age = 17,
                    Income = 8000000,
                    TaxCoe = 0.05
                },
                new Person()
                {
                    Id = 2,
                    Name = "Binh",
                    Age = 19,
                    Income = 9000000,
                    TaxCoe = 0.05
                }
            };
            return ListPerson;
        }
    }
}
