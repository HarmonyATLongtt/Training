using Bai_2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    internal class Student: IPerson
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Income { get; set; }
        public double TaxCoe { get; set; }
        public string Class {  get; set; }
        public string School { get; set; }

        public Student()
        {

        }

        public Student(string id, string name, int age, int income, string @class, string school)
        {
            Id = id;
            Name = name;
            Age = age;
            Income = income;
            TaxData taxData = new TaxData();
            TaxCoe = taxData.GetTaxCoe(age, income);
            Class = @class;
            School = school;
        }

        public string GetInfo()
        {
            return "\t" + Id + "\t" + Name + "\t" + Age + "\t" + School + "\t" + Class;
        }
    }
}
