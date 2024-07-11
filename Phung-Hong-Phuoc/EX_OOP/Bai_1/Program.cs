using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    internal class Program
    {
        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public double InCome { get; set; }
            public double TaxCoe { get; set; }

            public Person()
            { }

            public Person(int id, string name, int age, double inCome, double taxCoe)
            {
                Id = id;
                Name = name;
                Age = age;
                InCome = inCome;
                TaxCoe = taxCoe;
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
            List<Person> list = new List<Person> {
                new Person {Id = 1,Name= "nam",Age= 15,InCome= 2000,TaxCoe= 1.2 },
                new Person {Id = 2,Name= "thu",Age= 16,InCome= 2800,TaxCoe= 1.5 },
                new Person {Id = 3,Name= "hung",Age= 17,InCome= 1200,TaxCoe= 1.7 },
                new Person {Id = 4,Name= "hian",Age= 18,InCome= 30000,TaxCoe= 2.2 }
                };
            return list;
        }

        public static void OutPut(Person person)
        {
            Console.WriteLine($"Id:{person.Id}, Name : {person.Name}, Age : {person.Age}");
        }

        private static void Main(string[] args)
        {
            List<Person> list = Init();
            foreach (var item in list)
            {
                OutPut(item);
            };
            Console.ReadKey();
        }
    }
}