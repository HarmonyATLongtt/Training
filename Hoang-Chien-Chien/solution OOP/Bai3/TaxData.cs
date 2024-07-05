using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3
{
    public class TaxData
    {
        public static double GetTaxCoe(int age, double income)
        {
            if (age < 18)
            {
                return 0;
            }
            if (income <= 9000000) return 0.05;
            if (income > 9000000 && income <= 15000000) return 0.1;
            if (income > 15000000 && income <= 20000000) return 0.15;
            if (income > 20000000 && income <= 30000000) return 0.2;
            return 0;
        }
    }
}
