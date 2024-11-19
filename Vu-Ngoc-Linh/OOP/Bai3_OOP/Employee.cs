using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3_OOP
{
    public class Employee: Person
    {
        public string _company {  get; set; }
        public string _jobTitle { get; set; } 
        public int _income { get; set; }
        public double _taxCoe {  get; set; }
        public Employee() : base() { }

        public Employee(string id, string name, int age, string company, string jobtitle, int income) : base(id, name, age)
        {
            _id = id;
            _name = name;
            _age = age;
            _company = company;
            _jobTitle = jobtitle;
            this._income = income;
            this._taxCoe = TaxData.GetTaxCoe(age, income);
        }
        public double GetTax()
        {
            return this._income * this._taxCoe;
        }
        public override void Init()
        {
            base.Init();
            _company = GetInput("Company: ");
            _jobTitle = GetInput("Job Title: ");
            this._income = int.Parse(GetInput("Income: "));
        }
        public override string GetInfo()
        {
            string tmp = base.GetInfo();
            return $"{tmp},Company: {_company}, Job Title: {_jobTitle}, Income: {this._income}, Tax: {GetTax()}";
        }
    }
}
