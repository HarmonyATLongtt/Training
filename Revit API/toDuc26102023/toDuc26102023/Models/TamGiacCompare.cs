using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace toDuc26102023.Models
{
    public class TamGiacCompare : IComparer<Tamgiac>
    {
        public int Compare(Tamgiac tam1, Tamgiac tam2)
        {
            double Y1 = Math.Round(tam1.ChanDuongCao().Y, 7);
            double Y2 = Math.Round(tam2.ChanDuongCao().Y, 7);
            double X1 = Math.Round(tam1.ChanDuongCao().X, 7);
            double X2 = Math.Round(tam2.ChanDuongCao().X, 7);
            if (Y1 > Y2)
                return -1;
            else if (Y1 < Y2)
                return 1;
            else
            {
                if (X1 < X2)
                    return -1;
                else if (X1 > X2)
                    return 1;
                else return 0;
            }


        }
    }
}
