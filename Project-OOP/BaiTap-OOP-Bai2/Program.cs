using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiTap_OOP_Bai2
{
    public class TaxData {
        public static double GetTaxCoe(int age, double income)
        {
            if(age < 0)
            {
                return 0;
            }
            else if (income <= 9000000)
            {
                return 0.05;
            }
            else if (income <= 15000000)
            {
                return 0.1;
            }
            else if (income <= 20000000)
            {
                return 0.15;
            }
            else if (income <= 30000000)
            {
                return 0.2;
            }
            else
            {
                return 0.25;
            }
        }
    }

    class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }

        public Person(int id, string name, int age, double income, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = TaxData.GetTaxCoe(age, income);
        }

        public bool Equals(Person p)
        {
            if (p == null) return false;
            return Id == p.Id && Name == p.Name && Age == p.Age && Income == p.Income && TaxCoe == p.TaxCoe;
            
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
            Program.SoSanh();
        }

        static void SoSanh()
        {
            if (peopleList[0].Equals(peopleList[1]))
            {
                Console.WriteLine("Nguoi dung da ton tai");
            }
            else Console.WriteLine("chua so sanh");
            Console.ReadLine();
        }
        static void Init()
        {
            TaxData taxData = new TaxData();
            peopleList.Add(new Person (1, "Nguyen Truong Giang", 22, 15000000, taxData ));
            peopleList.Add(new Person (1, "Nguyen Truong Giang", 22, 15000000, taxData ));
            peopleList.Add(new Person ( 2, "Nguyen Linh Giang", 23, 3000000, taxData ));
            peopleList.Add(new Person ( 3, "Nguyen Trung",22, 25000000, taxData ));
            peopleList.Add(new Person ( 4, "Tran The Long", 28, 30000000, taxData ));
        }

        static void Output()
        {
            foreach (var person in peopleList)
            {
                Console.WriteLine($"Id: {person.Id}, Name: {person.Name}, Tax: {person.GetTax()}");
            }
            Console.ReadLine();
        }
    }
}
