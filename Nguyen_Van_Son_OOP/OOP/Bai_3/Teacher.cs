using Bai_2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    internal class Teacher : Person, IPerson
    {
        public string School;
        public Teacher() { }

        public Teacher(int id, string? name, int age, float income, float taxCoe, string school)
            : base(id, name, age, income, taxCoe)
        {
            this.School = school;
        }
        public Teacher(int id, string? name, int age, float income, TaxData taxData, string school)
            : base(id, name, age, income, taxData)
        {
            this.School = school;
            this.TaxCoe = taxData.GetTaxCoe(age, income);
        }
        public static void Title()
        {
            Console.WriteLine($"{"Id",-5}{"Name",-15}{"Age",-5}{"School",-10}{"Income",-15}{"Tax"}");
        }
        public void GetInfo()
        {
            Console.WriteLine($"{this.Id,-5}{this.Name,-15}{this.Age,-5}{this.School,-10}{this.Income,-15}{this.GetTax()}");
        }
    }
}
