using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bai_2
{
    class TaxData
    {
        public int Age { get; set; }
        public float Income { get; set; }

        public float GetTaxCoe()
        {
            if (Age < 18)
            {
                return 0;
            }
            else {
                switch (Income)
                {
                    case (<= 9000000):
                        return 0.05F;
                    case (> 9000000 and <= 15000000):
                        return 0.10F;
                    case (> 15000000 and <= 20000000):
                        return 0.15F;
                    case (> 20000000 and <= 30000000):
                        return 0.20F;
                    default: return 0.50F;
                }
                
            }
        }
    }

    class person
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public float Income { get; set; }

        public float Tax_coe { get; set; }

        public person(string id, string name, string age, string income,TaxData mytax)
        {
            Id = id;
            Name = name;
            Age = Convert.ToInt32(age);
            Income = float.Parse(income);
            mytax.Age = Convert.ToInt32(age);
            mytax.Income = float.Parse(income);
            Tax_coe = mytax.GetTaxCoe();
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
            return tax;
        }
    }

    
}
