using Bai_2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    internal class Student : Person, IPerson
    {
        public string Class;
        public string School;
        public Student() { }

        public Student(int id, string? name, int age, float income, float taxCoe, string Class, string school)
            : base(id, name, age, income, taxCoe)
        {
            this.Class = Class;
            this.School = school;
        }
        public Student(int id, string? name, int age, float income, TaxData taxData, string Class, string school)
            : base(id, name, age, income, taxData)
        {
            this.Class = Class;
            this.School = school;
        }
        public static void Title()
        {
            Console.WriteLine($"{"Id",-5}{"Name",-15}{"Age",-5}{"School",-10}{"Class"}");
        }
        public void GetInfo()
        {
            Console.WriteLine($"{this.Id,-5}{this.Name,-15}{this.Age,-5}{this.School,-10}{this.Class}");
        }
    }
}
