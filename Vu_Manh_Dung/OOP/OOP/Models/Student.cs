using System;

namespace OOP.Models
{
    public class Student : Person
    {
        public string Class { get; set; }
        public School School = new School();
        public Student()
        { }
        public Student(string id, string name, int age, double income, string _class, string schoolname):base(id, name, age, income)
        {
            Class = _class;
            School.Name = schoolname;
        }
        public static void Title()
        {
            Person.Title();
            Console.WriteLine($"{"Class",10}{"School",10}");
        }
        public override void GetInfo()
        {
            base.GetInfo();
            Console.WriteLine($"{Class,10}{School.Name,10}");
        }
    }
}