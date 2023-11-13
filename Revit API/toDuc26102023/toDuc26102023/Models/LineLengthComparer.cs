using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace toDuc26102023.Models
{
    public class LineLengthComparer : IComparer<ModelLine>
    {
        public int Compare(ModelLine line1, ModelLine line2)
        {
            double length1 = line1.GeometryCurve.Length;
            double length2 = line2.GeometryCurve.Length;

            if (length1 > length2)
                return -1;
            else if (length1 < length2)
                return 1;
            else
                return 0;
        }
    }
}
