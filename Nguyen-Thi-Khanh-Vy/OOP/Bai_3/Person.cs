using OPP.Bai_3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Person 
    {
        protected string _id { get; set; }
        protected string _name { get; set; }
        protected int _age { get; set; }
        protected double _income { get; set; }
        protected double _taxcoe { get; set; }

        public Person()
        {

        }
        public Person(string id, string name, int age, double income, double taxcoe)
        {
            _id = id;
            _name = name;
            _age = age;
            _income = income;
            _taxcoe = taxcoe;
        }
        public void Nhap()
        {
           
        }
    }
}

