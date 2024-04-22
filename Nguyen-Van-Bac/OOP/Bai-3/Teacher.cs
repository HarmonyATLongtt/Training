using System;
using System.Collections.Generic;
using System.Text;

namespace Bai_3
{
    public class Teacher : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }

        public string GetInfo()
        {
            return $"ID: {Id}, Name: {Name}, Age: {Age}, School: {School}, Income: {Income}, Tax: {Income * TaxCoe}";
        }
    }
}
