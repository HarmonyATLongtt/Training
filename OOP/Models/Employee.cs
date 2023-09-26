using NguyenVanViet.Core;
using System;

namespace NguyenVanViet.Models
{
    // Create class Employee extends Person and implement interface IPerson
    public class Employee : Person, IPerson
    {
        public string Company { get; set; }
        public string JobTitle { get; set; }

        public Employee(int id, string name, int age, float income, string company, string jobTitle, TaxData taxData)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            Company = company;
            JobTitle = jobTitle;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public void GetInfo()
        {
            Console.WriteLine($"ID: {Id}, Name: {Name}, Age: {Age}, Company: {Company}, Job title: {JobTitle}, Income: {Income}, Tax: {TaxCoe}");
        }
    }
}