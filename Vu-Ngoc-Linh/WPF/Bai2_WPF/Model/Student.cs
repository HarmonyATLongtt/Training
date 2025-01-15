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
        public int STT { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string ID { get; set; }
        public string DOB { get; set; }
        public string Class { get; set; }
        public string School { get; set; }
        public Student() : base() { }

        public Student(int stt, string name, int age, string id, string dob,  string school, string lop) : base(stt, name, age, id, dob)
        {
            this.School = school;
            this.Class = lop;
        }
    }
}
