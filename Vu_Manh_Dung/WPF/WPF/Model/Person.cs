using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Model
{
    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public float Taxcoe { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}\nName: {Name}\nAge: {Age}\nAddress: {Address}\nTaxcoe: {Taxcoe}";
        }
    }
}
