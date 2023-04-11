using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_1
{
    public class TaxData : Person
    {
        public double GetTaxCoe(int Age, double Income)
        {
            if (Age < 18)
            {
                return TaxCoe = 0;
            }
            else
            {
                if (Income <= 9000000)
                {
                    return TaxCoe = 0.05;
                }
                else if (Income <= 15000000)
                {
                    return TaxCoe = 0.1;
                }
                else if (Income <= 20000000)
                {
                    return TaxCoe = 0.15;
                }
                else if (Income <= 30000000)
                {
                    return TaxCoe = 0.2;
                }
                else
                {
                    return TaxCoe = 0;
                }
            }
        }
    }
}