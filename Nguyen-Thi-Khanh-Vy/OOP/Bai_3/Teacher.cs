using Bai_3;
using OOP.Bai_3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OOP.Bai_3
{
    public class Teacher: IPerson
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public double Income { get; set; }
        public double TaxCoe {  get; set; }
        public string School {  get; set; }
        private TaxData taxData;
        public Teacher(string id, string name, int age, string school, double income, TaxData taxData)
        {
            ID = id;
            Name = name;
            Age = age;
            School = school;
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
                    Console.WriteLine("Gia tri nhap khong duoc de trong.Vui long nhap lai!");
                }
            }while (string.IsNullOrWhiteSpace(input));
            return input;
        }
       
        public double GetTax()
        {
            return Income * TaxCoe;
        }
        public string GetInfo()
        {
            return $"ID: {ID}, Name: {Name}, Age: {Age}, School: {School}, Income: {Income:C}, Tax: {GetTax():C}";
        }
        public void Nhap()
        {
            ID = GetInput("Nhap ID:");
            Name = GetInput("Nhap Ho ten: ");

            while (true)
            {
                string ageInput = GetInput("Nhap tuoi: ");
                if (int.TryParse(ageInput, out int age) && age > 0)
                {
                    Age = age;
                    break;
                }Console.WriteLine("Tuoi khong hop le vui long nhap lai");
            }
            School = GetInput("Nhap truong: ");
            while (true)
            {
                string incomeInput = GetInput("Nhap Income: ");
                if (double.TryParse(incomeInput, out double income) && income >= 0)
                {
                    Income = income;
                    break;
                }
                Console.WriteLine("Income khong hop le vui long nhap lai!");
            }
            TaxCoe = taxData.GetTaxCoe(Age, Income);

        }
    }
}
