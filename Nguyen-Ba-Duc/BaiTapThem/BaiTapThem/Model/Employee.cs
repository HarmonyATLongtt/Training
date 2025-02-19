using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BaiTapThem.ViewModel;

namespace BaiTapThem.Model
{
    public class Employee : Person
    {
        public string? Company { get; set; }

        public string? JobTitle { get; set; }

        public Employee()
        { }

        public Employee(int id, string name, int age, int income, double tax, string company, string jobtitle)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = tax;
            Company = company;
            JobTitle = jobtitle;
        }
    }
}