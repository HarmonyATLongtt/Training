using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using FirstCommand.Support.Constants;

namespace FirstCommand.Support.PointHandle
{
    public static class PointUtility
    {
        /// <summary>
        /// Hàm dùng để nhóm các điểm gần nhau thành 1 nhóm
        /// </summary>
        /// <param name="points"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static List<List<XYZ>> GroupClosePoints(List<XYZ> points, double length)
        {
            var groups = new List<List<XYZ>>();
            var visited = new HashSet<XYZ>();

            foreach (var p in points)
            {
                if (visited.Contains(p)) continue;

                var group = new List<XYZ> { p };
                visited.Add(p);

                // Hàng đợi để duyệt theo BFS
                var queue = new Queue<XYZ>();
                queue.Enqueue(p);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();

                    foreach (var other in points)
                    {
                        if (!visited.Contains(other) && current.DistanceTo(other) <= length)
                        {
                            group.Add(other);
                            visited.Add(other);
                            queue.Enqueue(other);
                        }
                    }
                }

                groups.Add(group);
            }

            return groups;
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
        /// Hàm lấy BoundingBoxXYZ bao toàn bộ một mô hình gồm nhiều Element và trả về 2 điểm min max
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static (XYZ minPoint, XYZ maxPoint) GetOverallBoundingBox(Document doc, List<Element> elements)
        {
            XYZ min = null;
            XYZ max = null;

            foreach (var element in elements)
            {
                BoundingBoxXYZ bbox = element.get_BoundingBox(null);
                if (bbox == null) continue;

                XYZ minPt = bbox.Min;
                XYZ maxPt = bbox.Max;

                if (min == null)
                {
                    min = new XYZ(minPt.X, minPt.Y, minPt.Z);
                    max = new XYZ(maxPt.X, maxPt.Y, maxPt.Z);
                }
                else
                {
                    min = new XYZ(
                        Math.Min(min.X, minPt.X),
                        Math.Min(min.Y, minPt.Y),
                        Math.Min(min.Z, minPt.Z));

                    max = new XYZ(
                        Math.Max(max.X, maxPt.X),
                        Math.Max(max.Y, maxPt.Y),
                        Math.Max(max.Z, maxPt.Z));
                }
            }

            return (min, max);
        }

        /// <summary>
        /// Hàm kiểm tra 1 điểm có nằm trong phạm vi nào đó, phạm vi đó được đại diện bởi danh sách các điểm
        /// </summary>
        /// <param name="p"></param>
        /// <param name="points"></param>
        /// <param name="gap"></param>
        /// <returns></returns>
        public static bool IsPointNearListPoint(XYZ p, List<XYZ> points, double gap)
        {
            foreach (XYZ point in points)
            {
                if (p.DistanceTo(point) < gap)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Hàm kiểm tra xem 1 điểm có thuộc 1 đường thẳng hay không
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool IsPointOnLine(XYZ point, Line line)
        {
            XYZ start = line.GetEndPoint(0);
            XYZ end = line.GetEndPoint(1);

            // Vector từ start đến end và từ start đến point
            XYZ lineVec = end - start;
            XYZ pointVec = point - start;

            // Nếu độ dài của lineVec là 0 (line sai), trả về false
            if (lineVec.IsZeroLength())
                return point.IsAlmostEqualTo(start, CommonConstants.TOLERANCE);

            // Kiểm tra xem hai vector có cùng hướng (tức là tích có hướng gần bằng 0)
            XYZ cross = lineVec.CrossProduct(pointVec);
            if (cross.GetLength() > CommonConstants.TOLERANCE)
                return false;

            if ((point.DistanceTo(end) + point.DistanceTo(start)) - start.DistanceTo(end) > CommonConstants.TOLERANCE)
                return false;

            return true;
        }

        /// <summary>
        /// Hàm lấy ra tâm của 1 tập hợp các điểm
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public static XYZ GetCenterPoint(List<XYZ> points)
        {
            if (points == null || points.Count == 0)
                throw new System.ArgumentException("Danh sách điểm rỗng");

            double sumX = 0;
            double sumY = 0;
            double sumZ = 0;

            foreach (var point in points)
            {
                sumX += point.X;
                sumY += point.Y;
                sumZ += point.Z;
            }

            int count = points.Count;
            return new XYZ(sumX / count, sumY / count, sumZ / count);
        }

        /// <summary>
        /// Hàm này dùng để set lại originpoint
        /// </summary>
        /// <param name="p"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static XYZ SetPointWithNewZValue(XYZ p, double z)
        {
            var result = new XYZ(p.X, p.Y, z);
            return result;
        }
    }
}