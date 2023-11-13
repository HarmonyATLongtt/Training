using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    class person
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }

        public float Tax_coe { get; set; }

        public person(string ID, string NAME, string AGE, string INCOME, string TAX_COE) { 
            Id = ID;
            Name = NAME;
            Age = Convert.ToInt32(AGE);
            Income = float.Parse(INCOME);
            Tax_coe = float.Parse(TAX_COE);
        }
        public bool Equals(person p)
        {
            if (p.Id == Id)
            {
                Console.WriteLine("Already available with {0}", p.Name);
                return false;
            }
            else
            {
                return true;
            }

        }
        public float GetTax()
        {
            float tax = Tax_coe * Income;
            return tax ;
        }
    }
}
