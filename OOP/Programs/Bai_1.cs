using NguyenVanViet.Models;
using OOP.Core;
using System;
using System.Collections.Generic;

#nullable enable

namespace OOP.Programs
{
    public class Bai_1 : IProgram
    {
        private List<Person>? li;

        public void Init()
        {
            li = new List<Person>
            {
                new Person(1, "Nguyen Van A", 19, 1250000, 0.3f),
                new Person(2, "Nguyen Van B", 20, 1000000, 0.1f),
                new Person(3, "Nguyen Van C", 18, 2100000, 0.4f),
                new Person(4, "Nguyen Van D", 21, 1500000, 0.2f),
                new Person(5, "Nguyen Van E", 19, 3000000, 0.5f)
            };
        }

        public void Output()
        {
            if (li != null)
                foreach (var item in li)
                {
                    Console.WriteLine("ID: {0}, Name: {1}, Tax: {2}", item.Id, item.Name, item.TaxCoe);
                }
            else
                Console.WriteLine("Don't contain data!");
        }
    }
}