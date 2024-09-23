using OOP.Bai_3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bai_3;

namespace OOP.Bai_3
{
    public class Employee : Person1
    {
        public double _income { get; set; }
        public double _taxCoe {  get; set; }
        public string _company {  get; set; }
        public string _jobTitle { get; set; }
        private TaxData taxData;

        public Employee(): base() { }

        public Employee(string iD, string name, int age):base( iD, name, age)
        {

        }
        public Employee(string iD, string name, int age, string company, string jobTitle, double income, TaxData taxData)
        {
            _id = iD;
            _name = name;
            _age = age;
            _company = company;
            _jobTitle = jobTitle;
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
            return $"{baseInfo}, Company: {_company}, JobTitle: {_jobTitle}, Income: {_income:C}, Tax: {GetTax():C}";
        }
        public override void Nhap()
        {
            base.Nhap();

            _company = GetInput("Nhap cty:");
            _jobTitle = GetInput("Nhập cong viec:");

            
            do
            {
                string incomeInput = GetInput("Nhập Income:");
                if (double.TryParse(incomeInput, out double income) && income >= 0)
                {
                    _income = income;
                    break;
                }
                Console.WriteLine("Income không hợp lệ. Vui lòng nhập lại:");
            }while (true);

            // Tính toán hệ số thuế
            _taxCoe = taxData.GetTaxCoe(_age, _income);
        }
    }
}
