using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_Ex
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string ID { get; set; }
        public string Address { get; set; }
        public double TaxCoe { get; set; }
    }

    public class Student : Person
    {
        
    }

    public class Teacher : Person
    {
        
    }

    public class Employee : Person
    {
       
    }

}
