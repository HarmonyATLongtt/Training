using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class Teacher:Person
    {
        public string School { get; set; }

        public Teacher(string id,string name,int age, int income, TaxData taxcoe, string school):base(id,name,age,income,taxcoe)
        {
            this.School = school;
        }

        public override string GetInfo()
        {
            return base.GetInfo() + $"{School}\t{Income}\t{Taxcoe}";
        }
    }
}
