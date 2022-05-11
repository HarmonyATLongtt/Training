using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerssionControl
{
    public class Teacher
    {
        private string _Name;
        private string _Description;
        private int _Age;
        public Teacher(string name, int age)
        {
            _Name = name;
            _Age = age;
        }
        public void ShowInfor()
        {
            Console.WriteLine(_Description);
            // comment for conflict resolving 
        }
    }
}
