using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3
{
    public class Student : IPerson
    {
        private int id;
        private string name;
        private int age;
        private string schoool;
        private string className;

        public Student() { }
        public Student(int id, string name, int age, string school, string className)
        {
            this.Id = id;
            this.Name = name;
            this.Age = age;          
            this.Schoool = school;
            this.ClassName = className;

        }

        public int Id { get { return id; } set { id = value; } }
        public string Name { get { return name; } set { name = value; } }
        public int Age { get { return age; } set { age = value; } }
        
        public string Schoool { get { return schoool; } set { schoool = value; } }
        public string ClassName { get { return className; } set { className = value; } }

        public string GetInfo()
        {
            return $"_{this.Id}_{this.Name}_{this.Age}_{this.Schoool}_{this.ClassName}";
        }
    }
}
