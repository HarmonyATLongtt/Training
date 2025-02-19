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
    public class Person
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int Age { get; set; }

        public double TaxCoe { get; set; }
        public int Income { get; set; }

        public Person()
        { }
    }
}