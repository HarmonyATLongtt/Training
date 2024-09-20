using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2_OOP
{
    public class Person
    {
        private string _id;
        public string Id { get; set; }
        private string _name;
        public string Name { get; set;}
        private int _age;
        public int Age { get; set; }
        private double _income;
        public double Income { get; set; }
        private string _taxCoe;
        public string TaxCoe { get; set; }
        public Person(string id, string name, int age, double income, TaxData taxCoe)
        {
            this.Id = id;
            this.Name = name;
            this.Age = age;
            this.Income = income;
            this.TaxCoe = taxCoe.GetTaxCoe(age, income);
        }
        public bool Equals()
        {
            return Id == this.Id && Name==this.Name && Age==this.Age && Income==this.Income && TaxCoe==this.TaxCoe;
        }
        public double GetTax()
        {
            double tmp;
            if (double.TryParse(this.TaxCoe.Substring(0,2), out tmp))
            {
                tmp *= this.Income/100;
            }else if(double.TryParse(this.TaxCoe.Substring(0,1),out tmp))
            {
                tmp*=this.Income/100;
            }
            return tmp;
        }
    }
}
