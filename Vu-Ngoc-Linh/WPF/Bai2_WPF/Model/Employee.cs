using Bai2_WPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Bai2_WPF.Model
{
    public class Employee : Person
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Job { get; set; }
        public int Income { get; set; }
        public double TaxCoe { get; set; }
        public double Tax { get; set; }
        public Employee() : base() { }

        public Employee(string id, string name, int age, string jobtitle, int income) : base(id, name, age)
        {
            ID = id;
            Name = name;
            Age = age;
            Job = jobtitle;
            this.Income = income;
            this.TaxCoe = TaxData.GetTaxCoe(age, income);
            this.Tax = GetTax();
        }
        public double GetTax()
        {
            return this.Income * this.TaxCoe;
        }
    }
}
