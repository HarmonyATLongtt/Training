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
        public double Income { get; set; }
        public double TaxCoe {  get; set; }
        public string School {  get; set; }
        private TaxData taxData;

        public Teacher (string id, string name, int age): base(id, name, age)
        {

        }
        public Teacher(string id, string name, int age, string school, double income, TaxData taxData)
        {
            _id = id;
            _name = name;
            _age = age;
            School = school;
            Income = income;
            this.taxData = taxData;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }
        public double GetTax()
        {
            return Income *TaxCoe;
        }
        public override string GetInfo()
        {
            var baseInfo = base.GetInfo();
            return $"{baseInfo}, School: {School}, Income: {Income:C}, Tax: {GetTax():C}";
        }
        public override void Nhap()
        {
            base.Nhap();
            School = GetInput("Nhap truong: ");
            bool isCheck = false; 
            do
            {
                string incomeInput = GetInput("Nhap Income: ");
                if (isCheck = double.TryParse(incomeInput, out double income) && income >= 0)
                {
                    Income = income;
                    break;
                }
                Console.WriteLine("Income khong hop le vui long nhap lai!");
            }while (!isCheck);
            TaxCoe = taxData.GetTaxCoe(_age, Income);

        }
    }
}
