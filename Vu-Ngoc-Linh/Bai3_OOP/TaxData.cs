using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Bai3_OOP
{
    public class TaxData
    {
        public static double GetTaxCoe(int age, int income)
        {
            double hSo = 0;
            if (age < 18)
                return hSo;
            else
            {
                if (income <= 9000000) hSo = 0.05;
                if (income <= 15000000) hSo = 0.1;
                if (income <= 20000000) hSo = 0.15;
                if (income <= 30000000) hSo = 0.2;
            }
            return hSo;
        }
    }
}
