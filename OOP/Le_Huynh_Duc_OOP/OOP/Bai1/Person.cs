using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai1
{
    internal class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }

        public bool Equals1(Person p)
        {
            //if (p.Name.Equals(this.Name) && p.Age==this.Age && p.Income==this.Income && p.TaxCoe==this.TaxCoe)
            //    return true;
            //return false;
            return p.Name.Equals(this.Name) && p.Age == this.Age && p.Income == this.Income && p.TaxCoe == this.TaxCoe;

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
