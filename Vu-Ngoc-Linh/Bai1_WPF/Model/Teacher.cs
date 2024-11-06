using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Bai1_WPF.Model
{
    public class Teacher : Person
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string School { get; set; }
        public int Income { get; set; }
        public double TaxCoe { get; set; }
        public double Tax {  get; set; }
        public Teacher() : base() { }

        public Teacher(string id, string name, int age, string school, int income) : base(id, name, age)
        {
            ID = id;
            Name = name;
            Age = age;
            this.School = school;
            this.Income = income;
            this.TaxCoe = TaxData.GetTaxCoe(age, income);
            this.Tax = GetTax();
        }
        public double GetTax()
        {
            return this.Income * this.TaxCoe;
        }
    }
}
