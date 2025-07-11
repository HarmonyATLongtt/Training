using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace FirstCommand.Support.GenericClass.ComparerUtils
{
    /// <summary>
    /// Class này dùng khi muốn key của 1 dictionary là  XYZ
    /// </summary>
    public class XYZComparer : IEqualityComparer<XYZ>
    {
        private readonly double _tolerance;

        public XYZComparer(double tolerance = 1e-6)
        {
            _tolerance = tolerance;
        }

        public bool Equals(XYZ a, XYZ b)
        {
            return a.IsAlmostEqualTo(b, _tolerance);
        }

        public int GetHashCode(XYZ point)
        {
            // Lượng hóa tọa độ trước khi tạo hash code để nhất quán với Equals
            int x = (int)Math.Round(point.X / _tolerance);
            int y = (int)Math.Round(point.Y / _tolerance);
            int z = (int)Math.Round(point.Z / _tolerance);

            int hash = 17;
            hash = hash * 31 + x;
            hash = hash * 31 + y;
            hash = hash * 31 + z;
            return hash;
        }
    }
}