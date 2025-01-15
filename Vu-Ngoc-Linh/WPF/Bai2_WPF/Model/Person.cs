using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2_WPF.Model
{
    public class Person
    {
        public int STT { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string ID { get; set; }
        public string DOB { get; set; } 

        public Person(int stt, string name, int age, string id, string dob)
        {
            STT = stt;
            ID = id;
            Name = name;
            Age = age;
            DOB = dob;
        }   
        public Person() { }
    }
}
