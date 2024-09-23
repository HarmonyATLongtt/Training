using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2_OOP
{
    public class TaxData 
    {
        public string GetTaxCoe(int age, double income)
        {
            if (age < 18)
            {
                return "0";
            }
            else
            {
                if (income <= 9000000) return "5%";
                if (income <= 15000000) return "10%";
                if (income <= 20000000) return "15%";
                if (income <= 30000000) return "20%";
            }
            return "INVALID";
        }
    }
}
