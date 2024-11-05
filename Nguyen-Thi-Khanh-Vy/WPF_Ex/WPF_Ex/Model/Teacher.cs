using System;

namespace WPF_Ex.Model
{
    public class Teacher : Person
    {
        public double Income { get; set; } // Thu nhập
        public double TaxCoe { get; set; } // Hệ số thuế
        public string School { get; set; } // Trường
        private TaxData taxData;

        public Teacher() { }

        public Teacher(string id, string name, int age, string school, double income, TaxData taxData)
            : base(id, name, age)
        {
            School = school;
            Income = income;
            this.taxData = taxData;
            TaxCoe = taxData.GetTaxCoe(age, income);
        }

        public double GetTax()
        {
            return Income * TaxCoe;
        }
    }
}
