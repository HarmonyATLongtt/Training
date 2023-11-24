using System;

namespace OOP.Models
{
    public class Student : Person
    {
        public string Class { get; set; }
        public string School { get; set; }
        public Student() { }
        public Student(string id, string name, int age, double income, string _class, string school):base(id, name, age, income)
        {
            Class = _class;
            School = school;
        }
        public override void Init()
        {
            base.Init();
            Console.Write("Class: "); Class = Console.ReadLine();
            Console.Write("School: "); School= Console.ReadLine();
        }
        public static void Title()
        {
            Person.Title();
            Console.WriteLine($"{"Class",10}{"School",10}");
        }
        public override void GetInfo()
        {
            base.GetInfo();
            Console.WriteLine($"{Class,10}{School,10}");
        }
    }
}