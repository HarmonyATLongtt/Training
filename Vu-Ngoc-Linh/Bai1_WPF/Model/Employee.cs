using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Bai1_WPF.Model
{
    public class Employee : Person
    {
        public string _jobTitle { get; set; }
        public int _income { get; set; }
        public double _taxCoe { get; set; }
        public Employee() : base() { }

        public Employee(string id, string name, int age, string jobtitle, int income) : base(id, name, age)
        {
            _id = id;
            _name = name;
            _age = age;
            _jobTitle = jobtitle;
            this._income = income;
            this._taxCoe = TaxData.GetTaxCoe(age, income);
        }
        public double GetTax()
        {
            return this._income * this._taxCoe;
        }
        //
    }
}
