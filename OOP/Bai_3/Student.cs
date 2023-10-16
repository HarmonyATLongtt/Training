using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Student : Person
    {
        public string Class { get; set; }
        public string School { get; set; }

        public Student(string id,  string name, int age, string school, string _class):base(id,name,age)
        {
            this.School = school;
            this.Class = _class;
        }

        override public string GetInfo()
        {
            return base.GetInfo() + $"{School}\t{Class}";
        }
    }
}
