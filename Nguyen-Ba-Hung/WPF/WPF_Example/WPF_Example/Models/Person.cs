using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Example.Models
{
    internal class Person
    {
        private string Id { get; set; }
        private string Name { get; set; }
        private int Age { get; set; }
        private string Address { get; set; }
        private double Taxcoe { get; set; }

        public Person()
        {
            
        }

        public Person(string id, string name, int age, string address, double taxcoe)
        {
            Id = id;
            Name = name;
            Age = age;
            Address = address;
            Taxcoe = taxcoe;
        }
    }
}
