using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace FirstCommand.Support
{
    public static class GeometryUtility
    {
        private const double tolerance = 1e-6;

        //___Hàm kiểm tra xem hình vuông và hình tam giác có giao nhau không
        //___Start ___
        public static bool AreTriangleAndSquareIntersecting(List<XYZ> triangle, List<XYZ> square)
        {
            if (triangle.Any(p => IsPointInPolygon(p, square))) return true;
            if (square.Any(p => IsPointInPolygon(p, triangle))) return true;

            for (int i = 0; i < 3; i++)
            {
                XYZ a1 = triangle[i];
                XYZ a2 = triangle[(i + 1) % 3];

                for (int j = 0; j < 4; j++)
                {
                    XYZ b1 = square[j];
                    XYZ b2 = square[(j + 1) % 4];

                    if (SegmentsIntersectProperly(a1, a2, b1, b2))
                        return true;
                }
            }

            return false;
        }

        private static bool SegmentsIntersectProperly(XYZ a1, XYZ a2, XYZ b1, XYZ b2)
        {
            if (!TryGetSegmentsIntersection(a1, a2, b1, b2, out XYZ intersection))
                return false;

            return !IsPointOnEndpoint(intersection, a1, a2) && !IsPointOnEndpoint(intersection, b1, b2);
        }

        private static bool IsPointOnEndpoint(XYZ pt, XYZ p1, XYZ p2)
        {
            return IsSamePoint(pt, p1) || IsSamePoint(pt, p2);
        }

        private static bool IsSamePoint(XYZ a, XYZ b, double tol = 1e-6)
        {
            return a.DistanceTo(b) < tol;
        }

        private static bool IsPointInPolygon(XYZ point, List<XYZ> polygon)
        {
            int n = polygon.Count;
            bool inside = false;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                XYZ pi = polygon[i];
                XYZ pj = polygon[j];

                if (((pi.Y > point.Y) != (pj.Y > point.Y)) &&
                    (point.X < (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y + 1e-10) + pi.X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private static bool TryGetSegmentsIntersection(XYZ p1, XYZ p2, XYZ q1, XYZ q2, out XYZ intersection)
        {
            intersection = null;

            double A1 = p2.Y - p1.Y;
            double B1 = p1.X - p2.X;
            double C1 = A1 * p1.X + B1 * p1.Y;

            double A2 = q2.Y - q1.Y;
            double B2 = q1.X - q2.X;
            double C2 = A2 * q1.X + B2 * q1.Y;

            double det = A1 * B2 - A2 * B1;
            if (Math.Abs(det) < 1e-9)
                return false;

            double x = (B2 * C1 - B1 * C2) / det;
            double y = (A1 * C2 - A2 * C1) / det;

            XYZ pt = new XYZ(x, y, 0);

            if (IsPointOnSegment(pt, p1, p2) && IsPointOnSegment(pt, q1, q2))
            {
                intersection = pt;
                return true;
            }

            return false;
        }

        private static bool IsPointOnSegment(XYZ pt, XYZ a, XYZ b, double tol = 1e-6)
        {
            double ab = a.DistanceTo(b);
            double ap = a.DistanceTo(pt);
            double pb = pt.DistanceTo(b);
            return Math.Abs(ap + pb - ab) < tol;
        }

        //___End ___

        /// <summary>
        /// Hàm dùng đề chia mặt phẳng XY thành các grid, sau đó lấy ra danh sách các đỉnh của hình chữ nhật tạo bởi các grid
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static List<List<XYZ>> GenerateGridSquares(double minX, double maxX, double minY, double maxY, double gridSize)
        {
            List<List<XYZ>> squares = new List<List<XYZ>>();

            for (double x = minX; x < maxX; x += gridSize)
            {
                for (double y = minY; y < maxY; y += gridSize)
                {
                    var p1 = new XYZ(x, y, 0);
                    var p2 = new XYZ(x + gridSize, y, 0);
                    var p3 = new XYZ(x + gridSize, y + gridSize, 0);
                    var p4 = new XYZ(x, y + gridSize, 0);

                    squares.Add(new List<XYZ> { p1, p2, p3, p4 });
                }
            }

            return squares;
        }

        /// <summary>
        ///  Hàm lấy ra 2 điểm lớn nhất và nhỏ nhất của 1 element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static (XYZ minPoint, XYZ maxPoint) GetBoundingBoxExtents(Element element)
        {
            BoundingBoxXYZ bbox = element.get_BoundingBox(null); // null để lấy theo view 3D (toàn cục)
            if (bbox == null)
                return (null, null);

            XYZ min = bbox.Min;
            XYZ max = bbox.Max;

            return (min, max);
        }

        /// <summary>
        /// Hàm kiểm tra xem face có vuông góc với trục Z hay không
        /// </summary>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static bool IsFacePerpendicularToZ(XYZ normal)
        {
            double dot = normal.Normalize().DotProduct(XYZ.BasisZ);
            return Math.Abs(Math.Abs(dot) - 1) < tolerance;
        }
    }

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