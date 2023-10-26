using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai3
{
    internal class TaxData
    {
        public double GetTaxCoe(int age, double Income)
        {
            if (age < 18)
            {
                return 0;
            }
            else
            {
                if (Income <= 9000000)
                    return 0.05;
                else if (Income <= 15000000)
                    return 0.1;
                else if (Income <= 20000000)
                    return 0.15;
                else if (Income <= 30000000)
                    return 0.2;
                else return 0.3;
            }
        }
    }
}
