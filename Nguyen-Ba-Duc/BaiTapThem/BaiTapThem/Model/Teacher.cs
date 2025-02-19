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
    public class Teacher : Person
    {
        public string? School { get; set; }

        public Teacher()
        { }

        public Teacher(int id, string name, int age, int income, double tax, string school)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxCoe = tax;
            School = school;
        }
    }
}