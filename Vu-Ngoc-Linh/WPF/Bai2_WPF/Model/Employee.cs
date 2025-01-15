using Bai2_WPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Bai2_WPF.Model
{
    public class Employee : Person
    {
        public int STT { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string ID { get; set; }
        public string DOB { get; set; }
        public string Company { get; set; }
        public string Team { get; set; }
        public string Role { get; set; }
        public int Income { get; set; }
        public double TaxCoe { get; set; }
        public double Tax { get; set; }
        public Employee() : base() { }

        public Employee(int stt, string name, int age, string id, string dob, string company, string team,  string jobtitle, int income) : base(stt, name, age, id, dob)
        {
            Company = company;
            Team = team;
            Role = jobtitle;
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
