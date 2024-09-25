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
        public string Class {  get; set; }
        public string School {  get; set; }

        public Student(): base() { }

        public Student(string id, string name, int age) : base (id, name, age) 
        {
        }
        public Student(string id, string name, int age, string school, string @class)
        {
            _id = id;
            _name = name;
            _age = age;
           School = school;
            Class = @class;
        }
        public override void Nhap()
        {
            base.Nhap();
           School = GetInput("Nhap truong: ");
            Class = GetInput("Nhap lop: ");
        }
        public override string GetInfo()
        {
            var baseInfo = base.GetInfo();
            return $"{baseInfo}, School: {School}, Class: {Class}";
        }
    }
}
