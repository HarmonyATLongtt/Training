using System;
using System.Collections.Generic;
using System.Text;

namespace Bai_3
{
    public class Employee : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Company { get; set; }
        public string JobTitle { get; set; }
        public double Income { get; set; }
        public double TaxCoe { get; set; }

        public string GetInfo()
        {
            return $"ID: {Id}, Name: {Name}, Age: {Age}, Company: {Company}, Job Title: {JobTitle}, Income: {Income}, Tax: {Income * TaxCoe}";
        }
    }
}
