using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Teacher:Person
    {
        public School school = new School();

        public Teacher() { }

        public Teacher(string id,string name,int age, int income, TaxData taxcoe, string schoolname):base(id,name,age,income,taxcoe)
        {
            school.Name = schoolname;
        }

        public override void Init()
        {
            base.Init();
            Console.Write("School: ");
            school.Name = Console.ReadLine();
        }

        public static void Title()
        {
            Person.Title();
            Console.WriteLine(String.Format($"{"School",20}{"Income",20}{"Taxcoe",20}"));

        }

        override public void GetInfo()
        {
            base.GetInfo();
            Console.WriteLine(String.Format($"{school.Name,20}{Income,20}{Taxcoe,20}"));
        }
    }
}
