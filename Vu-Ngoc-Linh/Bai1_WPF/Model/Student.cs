using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai1_WPF.Model
{
    public class Student : Person
    {
        public string _class { get; set; }
        public string _school { get; set; }
        public Student() : base() { }

        public Student(string id, string name, int age, string lop, string school) : base(id, name, age)
        {
            _id = id;
            _name = name;
            _age = age;
            _class = lop;
            this._school = school;
        }

    }
}
