using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Student : Person
    {
        public string? Class { get; set; }
        public School school = new School();

        public Student() { }
        public Student(string id,  string name, int age, string schoolname, string _class):base(id,name,age)
        {
            school.Name = schoolname;
            this.Class = _class;
        }

        public override void Init()
        {
            base.Init();
            Console.Write("School: ");
            school.Name = Console.ReadLine();
            Console.Write("Class: ");
            Class = Console.ReadLine();
        }

        public static void Title()
        {
            Person.Title();
            Console.WriteLine(String.Format($"{"School",20}{"Class",10}"));
        }

        override public void GetInfo()
        {
            base.GetInfo();
            Console.WriteLine(String.Format($"{school.Name,20}{Class,10}"));
        }
    }
}
