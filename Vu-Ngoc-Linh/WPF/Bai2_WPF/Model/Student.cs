using Bai2_WPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2_WPF.Model
{
    public class Student : Person
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Class { get; set; }
        public string School { get; set; }
        public Student() : base() { }

        public Student(string id, string name, int age, string lop, string school) : base(id, name, age)
        {
            this.ID = id;
            this.Name = name;
            this.Age = age;
            this.Class = lop;
            this.School = school;
        }

    }
}
