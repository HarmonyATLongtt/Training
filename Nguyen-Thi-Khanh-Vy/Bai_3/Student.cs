using Bai_3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OOP.Bai_3;

namespace Bai_3
{
    public class Student : Person1
    {
        public string _class {  get; set; }
        public string _school {  get; set; }

        public Student(): base() { }

        public Student(string id, string name, int age) : base (id, name, age) 
        {
        }
        public Student(string id, string name, int age, string school, string @class)
        {
            _id = id;
            _name = name;
            _age = age;
            _school = school;
            _class = @class;
        }
        public override void Nhap()
        {
            base.Nhap();
            _school = GetInput("Nhap truong: ");
            _class = GetInput("Nhap lop: ");
        }
        public override string GetInfo()
        {
            var baseInfo = base.GetInfo();
            return $"{baseInfo}, School: {_school}, Class: {_class}";
        }
    }
}
