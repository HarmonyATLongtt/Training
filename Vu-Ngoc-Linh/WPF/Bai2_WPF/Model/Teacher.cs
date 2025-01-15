using Bai2_WPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Bai2_WPF.Model
{
    public class Teacher : Person
    {
        public int STT { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string ID { get; set; }
        public string DOB { get; set; }
        public string School { get; set; }
        public string Class { get; set; }
        public int Income { get; set; }
        public double TaxCoe { get; set; }
        public double Tax { get; set; }
        public Teacher() : base() { }

        public Teacher(int stt, string name, int age,string id,  string dob, string school, string lop, int income) : base(stt, name, age, id, dob)
        {
            this.School = school;
            this.Class = lop;
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
