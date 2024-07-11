using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace EX_OOP
{
    internal class Program
    {
        public class TaxData
        {
            public double GetTaxCoe(int age, double In)
            {
                if (age < 18)
                {
                    return 0;
                }
                else
                {
                    if (In <= 9000000)
                    {
                        return 0.05;
                    }
                    else if (In <= 15000000)
                    {
                        return 0.10;
                    }
                    else if (In <= 20000000)
                    {
                        return 0.15;
                    }
                    else if (In <= 30000000)
                    {
                        return 0.20;
                    }
                    else
                    {
                        return 0.25;
                    }
                }
            }
        }

        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public double InCome { get; set; }
            public double TaxCoe { get; set; }

            public Person()
            { }

            public Person(int id, string name, int age, double inCome, TaxData tax)
            {
                Id = id;
                Name = name;
                Age = age;
                InCome = inCome;
                TaxCoe = tax.GetTaxCoe(age, inCome);
            }

            public bool Equals(Person other)
            {
                if (other == null) return false;
                if (this.Id != other.Id || this.Name != other.Name ||
                    this.Age != other.Age || this.InCome != other.InCome || this.TaxCoe != other.TaxCoe) return false;
                return true;
            }

            public double GetTax()
            {
                return TaxCoe * InCome;
            }
        }

        public static List<Person> Init()
        {
            TaxData tax = new TaxData();
            List<Person> list = new List<Person> {
                new Person(1, "nam", 20, 200000, tax),
                new Person(2, "thu", 16, 2800, tax),
                new Person(3, "hung", 27, 120000000, tax),
                new Person(4, "hian", 18, 30000, tax)
                };
            return list;
        }

        public static void OutPut(Person person)
        {
            Console.WriteLine($"Id:{person.Id}, Name : {person.Name}, Age : {person.Age}, TaxCoe {person.TaxCoe}");
        }

        private static void Main(string[] args)
        {
            List<Person> ls = Init();
            foreach (var item in ls)
            {
                OutPut(item);
            }

            Console.ReadKey();
        }
    }
}