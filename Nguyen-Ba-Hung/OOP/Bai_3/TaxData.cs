using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_2
{
    internal class TaxData
    {
        public TaxData() 
        {
        }

        public double GetTaxCoe(int age, int income)
        {
            double taxCoe = 0;

            if(age < 18)
            {
                return 0;
            }
            else
            {
                if(income <= 9000000)
                {
                    taxCoe = 0.05;
                }
                else if(9000000 < income && income <= 15000000)
                {
                    taxCoe = 0.1;
                }
                else if (15000000 < income && income <= 20000000)
                {
                    taxCoe = 0.15;
                }
                else if (20000000 < income && income <= 30000000)
                {
                    taxCoe = 0.2;
                }
                
            }

            return taxCoe;
        }
    }
}
