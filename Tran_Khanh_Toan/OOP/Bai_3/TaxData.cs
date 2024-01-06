using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3
{
    public class TaxData
    {
        public double GetTaxData(int Age, double Income) { 
            if(Age < 18)
            {
                return 0;
            }else
            {
                if(Income <= 9000000)
                {
                    return 0.05;
                }else if(Income > 9000000 && Income <= 15000000)
                {
                    return 0.1;
                }else if(Income > 15000000 && Income <= 20000000)
                {
                    return 0.15;
                }else if(Income > 20000000 && Income <= 30000000)
                {
                    return 0.2;
                }else
                {
                    return 0.3;
                }
            }
        }
    }
}
