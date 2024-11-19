using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2_WPF.Model
{
    public class Person
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public Person(string id, string name, int age)
        {
            ID = id;
            Name = name;
            Age = age;
        }
        public Person() { }
    }
}
