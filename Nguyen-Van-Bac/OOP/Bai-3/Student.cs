using System;
using System.Collections.Generic;
using System.Text;

namespace Bai_3
{
    public class Student : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
        public string Class { get; set; }

        public string GetInfo()
        {
            return $"ID: {Id}, Name: {Name}, Age: {Age}, School: {School}, Class: {Class}";
        }
    }
}
