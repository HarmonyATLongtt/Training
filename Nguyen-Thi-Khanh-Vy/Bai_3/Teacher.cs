using Bai_3;
using OOP.Bai_3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Teacher : Person1
    {
        public double _income { get; set; }
        public double _taxCoe {  get; set; }
        public string _school {  get; set; }
        private TaxData taxData;

        public Teacher (string id, string name, int age): base(id, name, age)
        {

        }
        public Teacher(string id, string name, int age, string school, double income, TaxData taxData)
        {
            _id = id;
            _name = name;
            _age = age;
            _school = school;
            _income = income;
            this.taxData = taxData;
            _taxCoe = taxData.GetTaxCoe(age, income);
        }
        public double GetTax()
        {
            return _income * _taxCoe;
        }
        public override string GetInfo()
        {
            var baseInfo = base.GetInfo();
            return $"{baseInfo}, School: {_school}, Income: {_income:C}, Tax: {GetTax():C}";
        }
        public override void Nhap()
        {
            base.Nhap();
            _school = GetInput("Nhap truong: ");
            do
            {
                string incomeInput = GetInput("Nhap Income: ");
                if (double.TryParse(incomeInput, out double income) && income >= 0)
                {
                    _income = income;
                    break;
                }
                Console.WriteLine("Income khong hop le vui long nhap lai!");
            }while (true);
            _taxCoe = taxData.GetTaxCoe(_age, _income);

        }
    }
}
