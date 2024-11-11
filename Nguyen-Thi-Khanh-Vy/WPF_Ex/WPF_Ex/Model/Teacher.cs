using System;

namespace WPF_Ex.Model
{
    public class Teacher : Person
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; } // Trường
        public double Income { get; set; } // Thu nhập
        public double TaxCoe { get; set; } // Hệ số thuế
        public Teacher() { }

        public Teacher(string id, string name, int age, string school, double income, double taxcoe)
            : base(id, name, age)
        {
            School = school;
            Income = income;
            TaxCoe = taxcoe;
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }
    }
}
