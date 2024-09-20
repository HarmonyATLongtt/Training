using OOP.Bai_3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Employee: IPerson
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe {  get; set; }
        public string Company {  get; set; }
        public string JobTitle { get; set; }
        private TaxData taxData;

        public Employee() { }
        public Employee(string iD, string name, int age, string company, string jobTitle, double income, TaxData taxData)
        {
            ID = iD;
            Name = name;
            Age = age;
            Company = company;
            JobTitle = jobTitle;
            Income = income;
            this.taxData = taxData;
            TaxCoe = taxData.GetTaxCoe(age, income);

        }
        private static string GetInput(string prompt)
        {
            string input;
            do
            {
                Console.WriteLine(prompt);
                input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Giá trị không được để trống. Vui lòng nhập lại:");
                }
            } while (string.IsNullOrWhiteSpace(input));

            return input;
        }


        public double GetTax()
        {
            return Income * TaxCoe;
        }
        public string GetInfo()
        {
            return $"ID: {ID}, Name: {Name}, Age: {Age}, Company: {Company}, JobTitle: {JobTitle}, Income: {Income:C}, Tax: {GetTax():C}";
        }
        public void Nhap()
        {
            ID = GetInput("Nhập ID:");
            Name = GetInput("Nhập họ tên:");

          
            while (true)
            {
                string ageInput = GetInput("Nhập tuổi:");
                if (int.TryParse(ageInput, out int age) && age > 0)
                {
                    Age = age;
                    break;
                }
                Console.WriteLine("Tuổi không hợp lệ. Vui lòng nhập lại:");
            }

            Company = GetInput("Nhap cty:");
            JobTitle = GetInput("Nhập cong viec:");

            
            while (true)
            {
                string incomeInput = GetInput("Nhập Income:");
                if (double.TryParse(incomeInput, out double income) && income >= 0)
                {
                    Income = income;
                    break;
                }
                Console.WriteLine("Income không hợp lệ. Vui lòng nhập lại:");
            }

            // Tính toán hệ số thuế
            TaxCoe = taxData.GetTaxCoe(Age, Income);
        }
    }
}
