using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerssionControl
{
    public class Student
    {
        private string _Name;
        private string _Email;
        private int _Age;
        public Student(string name, string email, int age)
        {
            _Name = name;
            _Email = email; 
            _Age = age;
        }
        public void ShowInfor()
        {
            Console.WriteLine($"name : {_Name}, age: {_Age}");
        }

    }
}
