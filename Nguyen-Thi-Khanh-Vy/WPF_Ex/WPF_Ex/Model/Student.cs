using System;

namespace WPF_Ex.Model
{
    public class Student : Person
    {
        public string Class { get; set; } // Lớp
        public string School { get; set; } // Trường

        public Student() : base() { }

        public Student(string id, string name, int age, string school, string className)
            : base(id, name, age)
        {
            School = school;
            Class = className;
        }
    }
}
