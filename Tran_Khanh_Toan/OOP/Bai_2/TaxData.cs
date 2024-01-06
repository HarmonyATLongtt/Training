using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_2
{
    public class TaxData
    {
        public double GetTaxData(int Age, double InCome)
        {
            if(Age < 18)
            {
                return 0;
            }else {
                if(InCome <= 9000000)
                {
                    return 0.05;
                }else if(InCome > 9000000 && InCome <= 15000000)
                {
                    return 0.1;
                }else if(InCome > 15000000 && InCome <= 20000000)
                {
                    return 0.15;
                }else if(InCome > 20000000 && InCome <= 30000000)
                {
                    return 0.2;
                }
                else
                {
                    return 0.3;
                }
            }
        }
    }
}
