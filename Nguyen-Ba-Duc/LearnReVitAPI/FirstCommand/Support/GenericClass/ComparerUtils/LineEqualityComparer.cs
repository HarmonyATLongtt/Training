using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

using FirstCommand.Support.Constants;

namespace FirstCommand.Support.GenericClass.ComparerUtils
{
    public class LineEqualityComparer : IEqualityComparer<Line>
    {
        public bool Equals(Line l1, Line l2)
        {
            if (l1 == null || l2 == null) return false;

            return (l1.GetEndPoint(0).IsAlmostEqualTo(l2.GetEndPoint(0), CommonConstants.TOLERANCE) &&
                    l1.GetEndPoint(1).IsAlmostEqualTo(l2.GetEndPoint(1), CommonConstants.TOLERANCE)) ||
                   (l1.GetEndPoint(0).IsAlmostEqualTo(l2.GetEndPoint(1), CommonConstants.TOLERANCE) &&
                    l1.GetEndPoint(1).IsAlmostEqualTo(l2.GetEndPoint(0), CommonConstants.TOLERANCE)); // Cho phép ngược chiều
        }

        public int GetHashCode(Line l)
        {
            // Hashcode từ toạ độ, bất kể thứ tự
            var p1 = l.GetEndPoint(0);
            var p2 = l.GetEndPoint(1);

            long hash1 = GetPointHash(p1);
            long hash2 = GetPointHash(p2);

            // Bỏ thứ tự: dùng min + max
            return (hash1 + hash2).GetHashCode();
        }

        private long GetPointHash(XYZ p)
        {
            return (p.X * 73856093 + p.Y * 19349663 + p.Z * 83492791).GetHashCode();
        }
    }
}