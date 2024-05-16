using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class TaxData
    {

        public double GetTaxCoe(int age, double income)
        {
            if (age < 18)
            {
                return 0;
            }
            else
            {
                if (income <= 9000000)
                    return 0.05;
                else if (income <= 15000000)
                    return 0.1;
                else if (income <= 20000000)
                    return 0.15;
                else if (income <= 30000000)
                    return 0.2;
                else
                    return 0;
            }

        }
    }
}
