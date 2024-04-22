using System;
using System.Collections.Generic;
using System.Text;

namespace Bai_2
{
    public class TaxData
    {
        public double GetTaxCoe(int age, double income)
        {
            if (age < 18)
            {
                return 0;
            }
            if (income <= 9000000)
            {
                return 0.05;
            }
            else if (income <= 15000000)
            {
                return 0.1;
            }
            else if (income <= 20000000)
            {
                return 0.15;
            }
            else if (income <= 30000000)
            {
                return 0.2;
            }
            else
            {
                return 0.25;
            }
        }
    }
}
