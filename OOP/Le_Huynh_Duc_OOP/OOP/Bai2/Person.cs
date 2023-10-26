using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2
{
    internal class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }

        public Person (int id, string name, int age, double income, TaxData instance)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = instance.GetTaxCoe(age, income);
        }
        public bool Equals(Person p)
        {
            if (p.Name.Equals(this.Name) && p.Age == this.Age && p.Income == this.Income && p.TaxCoe == this.TaxCoe)
                return true;
            return false;
        }

        public double GetTax()
        {
            return TaxCoe * Income;
        }

        public void Input()
        {
            Console.Write("Id: ");
            Id = int.Parse(Console.ReadLine());
            Console.Write("Name: ");
            Name = Console.ReadLine();
            Console.Write("Age: ");
            Age = int.Parse(Console.ReadLine());
            Console.Write("Income: ");
            Income = double.Parse(Console.ReadLine());
            Console.Write("TaxCoe: ");
            TaxCoe = double.Parse(Console.ReadLine());
        }
    }
}
