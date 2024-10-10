using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai1_WPF.Model
{
    public class Person 
    {
        public string _id {  get; set; }
        public string _name {  get; set; }
        public int _age { get; set; }
        public Person(string id, string name, int age)
        {
            _id = id;
            _name = name;
            _age = age;
        }
        public Person() { }
    }
}
