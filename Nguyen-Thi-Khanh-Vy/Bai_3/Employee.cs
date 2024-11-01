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
        public double Income { get; set; }
        public double TaxCoe {  get; set; }
        public string Company {  get; set; }
        public string JobTitle { get; set; }
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
            Company = company;
            JobTitle = jobTitle;
            Income = income;
            this.taxData = taxData;
            TaxCoe = taxData.GetTaxCoe(age, income);

        }
        
        public double GetTax()
        {
            return Income * TaxCoe;
        }
        public override string GetInfo()
        {
            var baseInfo = base.GetInfo();
            return $"{baseInfo}, Company: {Company}, JobTitle: {JobTitle}, Income: {Income:C}, Tax: {GetTax():C}";
        }
        public override void Nhap()
        {
            base.Nhap();

            Company = GetInput("Nhap cty:");
            JobTitle = GetInput("Nhập cong viec:");

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
            } while (!isCheck);

            // Tính toán hệ số thuế
            TaxCoe = taxData.GetTaxCoe(_age, Income);
        }
    }
}
