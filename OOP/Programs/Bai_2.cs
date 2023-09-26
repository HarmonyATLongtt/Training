using NguyenVanViet.Core;
using NguyenVanViet.Models;
using OOP.Core;
using System;
using System.Collections.Generic;

#nullable enable

namespace OOP.Programs
{
    public class Bai_2 : IProgram
    {
        private List<Person>? li;

        public void Init()
        {
            TaxData taxData = new TaxData();
            li = new List<Person>
            {
                new Person(1, "Nguyen Van A", 19, 1250000, taxData),
                new Person(2, "Nguyen Van B", 20, 10000000, taxData),
                new Person(3, "Nguyen Van C", 18, 21000000, taxData),
                new Person(4, "Nguyen Van D", 21, 15000000, taxData),
                new Person(5, "Nguyen Van E", 19, 30000000, taxData)
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