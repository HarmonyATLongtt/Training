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
    public class Student : Person
    {
        public string? Class { get; set; }

        public string? School { get; set; }

        public Student()
        { }

        public Student(int id, string name, int age, string Class, string school)
        {
            this.Id = id;
            this.Name = name;
            this.Age = age;
            this.Class = Class;
            this.School = school;
        }
    }
}