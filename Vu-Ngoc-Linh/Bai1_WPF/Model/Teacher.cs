using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Bai1_WPF.Model
{
    public class Teacher : Person
    {
        public string _school { get; set; }
        public int _income { get; set; }
        public double _taxCoe { get; set; }
        public Teacher() : base() { }

        public Teacher(string id, string name, int age, string school, int income) : base(id, name, age)
        {
            _id = id;
            _name = name;
            _age = age;
            this._school = school;
            this._income = income;
            //this._taxCoe = TaxData.GetTaxCoe(age, income);
        }
        //public double GetTax()
        //{
        //    return this._income * this._taxCoe;
        //}
    }
}
