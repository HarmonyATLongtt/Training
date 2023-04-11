using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    internal class Person
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public decimal Income { get; set; }
        public decimal TaxCoe { get; set; } // hệ số thuế

        public bool Equals(Person p)
        {
            if (p == null)
            {
                return false;
            }

            return ID == p.ID && Name == p.Name && Age == p.Age && Income == p.Income && TaxCoe == p.TaxCoe;
        }

        public decimal GetTax(decimal Income, decimal TaxCoe)
        {
            return Income * TaxCoe;
        }
    }
}