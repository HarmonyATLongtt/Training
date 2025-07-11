using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using FirstCommand.Support.Constants;
using FirstCommand.Support.GeometryHandle;
using FirstCommand.Support.VectorHandle;

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

        /// <summary>
        /// Hàm kiểm tra xem 2 cặp điểm có điểm chung không
        /// </summary>
        /// <param name="pair1"></param>
        /// <param name="pair2"></param>
        /// <returns></returns>
        public static bool HasCommonPoint(UnorderedXYZPair a, UnorderedXYZPair b)
        {
            return a.Contains(b.Point1) || a.Contains(b.Point2);
        }

        /// <summary>
        /// Hàm kiểm tra xem 2 UnorderedXYZPair có cùng phương với nhau không
        /// </summary>
        /// <param name="pair1"></param>
        /// <param name="pair2"></param>
        /// <returns></returns>
        public static bool ArePairsParallel(UnorderedXYZPair pair1, UnorderedXYZPair pair2)
        {
            XYZ dir1 = (pair1.Point1 - pair1.Point2).Normalize();
            XYZ dir2 = (pair2.Point1 - pair2.Point2).Normalize();
            return VectorUtility.AreParallel(dir1, dir2, CommonConstants.TOLERANCE);
        }

        /// <summary>
        /// Hàm dùng để gom nhóm các UnorderedXYZPair thành các nhóm nhỏ có điểm chung và thẳng hàng
        /// </summary>
        /// <param name="pairs"></param>
        /// <returns></returns>
        public static List<List<UnorderedXYZPair>> GroupColinearPairs(List<UnorderedXYZPair> pairs)
        {
            List<List<UnorderedXYZPair>> groups = new List<List<UnorderedXYZPair>>();

            foreach (var pair in pairs)
            {
                bool addedToGroup = false;

                foreach (var group in groups)
                {
                    if (group.Any(existing =>
                        HasCommonPoint(pair, existing) &&
                        ArePairsParallel(pair, existing)))
                    {
                        group.Add(pair);
                        addedToGroup = true;
                        break;
                    }
                }

                if (!addedToGroup)
                {
                    groups.Add(new List<UnorderedXYZPair> { pair });
                }
            }

            return groups;
        }

        /// <summary>
        /// Hàm tìm ra 2 điểm xa nhau nhất trong 1 group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static (XYZ, XYZ) FindFurthestPointsInGroup(List<UnorderedXYZPair> group)
        {
            // 1. Gom tất cả điểm
            List<XYZ> points = new List<XYZ>();

            foreach (var pair in group)
            {
                if (!points.Any(p => p.IsAlmostEqualTo(pair.Point1)))
                    points.Add(pair.Point1);

                if (!points.Any(p => p.IsAlmostEqualTo(pair.Point2)))
                    points.Add(pair.Point2);
            }

            // 2. Tìm 2 điểm xa nhau nhất
            double maxDistance = 0;
            XYZ p1Max = null, p2Max = null;

            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    double dist = points[i].DistanceTo(points[j]);
                    if (dist > maxDistance)
                    {
                        maxDistance = dist;
                        p1Max = points[i];
                        p2Max = points[j];
                    }
                }
            }

            return (p1Max, p2Max);
        }

        /// <summary>
        /// Hàm tìm ra điểm gần nhất trong danh sách với 1 điểm cho trước
        /// </summary>
        /// <param name="points"></param>
        /// <param name="targetPoint"></param>
        /// <returns></returns>
        public static XYZ FindNearestPoint(List<XYZ> points, XYZ targetPoint)
        {
            double minDis = double.MaxValue;
            XYZ nearestPoint = null;
            foreach (var p in points)
            {
                if (p.DistanceTo(targetPoint) < minDis)
                {
                    minDis = p.DistanceTo(targetPoint);
                    nearestPoint = p;
                }
            }
            return nearestPoint;
        }

        /// <summary>
        /// Hàm tìm ra điểm xa nhất trong danh sách với 1 điểm cho trước
        /// </summary>
        /// <param name="points"></param>
        /// <param name="targetPoint"></param>
        /// <returns></returns>
        public static XYZ FindFarthestPoint(List<XYZ> points, XYZ targetPoint)
        {
            double maxDis = double.MinValue;
            XYZ farthestPoint = null;
            foreach (var p in points)
            {
                if (p.DistanceTo(targetPoint) > maxDis)
                {
                    maxDis = p.DistanceTo(targetPoint);
                    farthestPoint = p;
                }
            }
            return farthestPoint;
        }

        /// <summary>
        /// Hàm dùng để tìm ra 2 vector U,V vuông góc với direction và vuông góc với nhau để tạo 1 mặt phẳng
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="U"></param>
        /// <param name="V"></param>
        public static void BuildLocalUVFromDirection(XYZ direction, out XYZ U, out XYZ V)
        {
            XYZ W = direction.Normalize();
            // Tạo vector U sao cho không song song với W
            XYZ arbitrary = (Math.Abs(W.Z) < 0.99) ? XYZ.BasisZ : XYZ.BasisX;
            U = W.CrossProduct(arbitrary).Normalize();

            // Từ U và W => tìm V
            V = W.CrossProduct(U); // đảm bảo V vuông góc cả W và U
        }

        /// <summary>
        /// Mặt phẳng XY được chia thành các grid, mỗi grid có tọa độ x,y riêng
        /// Hàm sẽ kiểm tra xem point được truyên vào thuộc grid nào
        /// </summary>
        /// <param name="point"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static (int, int) GetGridIndex(XYZ point, double gridSize)
        {
            int ix = (int)Math.Floor(point.X / gridSize);
            int iy = (int)Math.Floor(point.Y / gridSize);
            return (ix, iy);
        }
    }

    /// <summary>
    /// Lớp này dùng để nhận vào 1 cặp điểm, có overide equals, gethashcode để instance của class có thể làm key trong dictionary
    /// </summary>
    public class UnorderedXYZPair
    {
        public XYZ Point1 { get; }
        public XYZ Point2 { get; }

        public UnorderedXYZPair(XYZ p1, XYZ p2)
        {
            Point1 = p1;
            Point2 = p2;
        }

        // Kiểm tra 1 điểm có nằm trong cặp không (gần đúng)
        public bool Contains(XYZ p)
        {
            return Point1.IsAlmostEqualTo(p, CommonConstants.TOLERANCE) || Point2.IsAlmostEqualTo(p, CommonConstants.TOLERANCE);
        }

        public override bool Equals(object obj)
        {
            if (obj is UnorderedXYZPair other)
            {
                return (Point1.IsAlmostEqualTo(other.Point1, CommonConstants.TOLERANCE) && Point2.IsAlmostEqualTo(other.Point2, CommonConstants.TOLERANCE)) ||
                       (Point1.IsAlmostEqualTo(other.Point2, CommonConstants.TOLERANCE) && Point2.IsAlmostEqualTo(other.Point1, CommonConstants.TOLERANCE));
            }
            return false;
        }

        public override int GetHashCode()
        {
            long hash1 = GetPointHash(Point1);
            long hash2 = GetPointHash(Point2);

            // Bỏ thứ tự bằng cách luôn cộng min + max
            long min = Math.Min(hash1, hash2);
            long max = Math.Max(hash1, hash2);

            return (min + max).GetHashCode();
        }

        private long GetPointHash(XYZ p)
        {
            return (p.X * 73856093 + p.Y * 19349663 + p.Z * 83492791).GetHashCode();
        }
    }
}