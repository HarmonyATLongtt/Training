using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai_3_test.ViewModel
{
    internal class FilterIntersection : IEqualityComparer<XYZ>
    {
        /// <summary>
        /// Filter intersections with identical X or Y coordinate values
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(XYZ x, XYZ y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return x.IsAlmostEqualTo(y);
        }
        /// <summary>
        /// get hash code of the compared object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(XYZ obj)
        {
            return 1;
        }
    }
}
