using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using FirstCommand.Support.Constants;
using FirstCommand.Support.PointHandle;

namespace FirstCommand.Support.GeometryHandle
{
    public static class GeometryUtility
    {
        #region Hàm kiểm tra xem hình vuông và hình tam giác có giao nhau không

        public static bool AreTriangleAndSquareIntersecting(List<XYZ> triangle, List<XYZ> square)
        {
            // 1. Đỉnh tam giác nằm trong hình vuông
            if (triangle.Any(p => IsPointInPolygon(p, square))) return true;
            // 2. Đỉnh hình vuông nằm trong tam giác
            if (square.Any(p => IsPointInPolygon(p, triangle))) return true;

            // 3. Cạnh giao nhau
            for (int i = 0; i < 3; i++)
            {
                XYZ a1 = triangle[i];
                XYZ a2 = triangle[(i + 1) % 3];

                for (int j = 0; j < 4; j++)
                {
                    XYZ b1 = square[j];
                    XYZ b2 = square[(j + 1) % 4];

                    //if (SegmentsIntersectProperly(a1, a2, b1, b2))
                    if (DoSegmentsIntersect(a1, a2, b1, b2))
                        return true;
                }
            }

            return false;
        }

        private static bool DoSegmentsIntersect(XYZ p1, XYZ p2, XYZ q1, XYZ q2)
        {
            return CCW(p1, q1, q2) != CCW(p2, q1, q2) && CCW(p1, p2, q1) != CCW(p1, p2, q2);
        }

        private static bool CCW(XYZ a, XYZ b, XYZ c)
        {
            return (c.Y - a.Y) * (b.X - a.X) > (b.Y - a.Y) * (c.X - a.X);
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

        #endregion Hàm kiểm tra xem hình vuông và hình tam giác có giao nhau không

        /// <summary>
        /// Hàm dùng đề chia mặt phẳng XY thành các grid, sau đó lấy ra danh sách các đỉnh của hình chữ nhật tạo bởi các grid
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        //public static List<List<XYZ>> GenerateGridSquares(double minX, double maxX, double minY, double maxY, double gridSize)
        //{
        //    List<List<XYZ>> squares = new List<List<XYZ>>();

        //    for (double x = minX; x < maxX; x += gridSize)
        //    {
        //        for (double y = minY; y < maxY; y += gridSize)
        //        {
        //            var p1 = new XYZ(x, y, 0);
        //            var p2 = new XYZ(x + gridSize, y, 0);
        //            var p3 = new XYZ(x + gridSize, y + gridSize, 0);
        //            var p4 = new XYZ(x, y + gridSize, 0);

        //            squares.Add(new List<XYZ> { p1, p2, p3, p4 });
        //        }
        //    }

        //    return squares;
        //}

        /// <summary>
        ///  Hàm dùng đề chia mặt phẳng XY thành các grid, sau đó lấy ra danh sách các đỉnh của hình chữ nhật tạo bởi các grid
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static List<List<XYZ>> GenerateGridSquares(double minX, double maxX, double minY, double maxY, double gridSize)
        {
            int cols = (int)Math.Ceiling((maxX - minX) / gridSize);
            int rows = (int)Math.Ceiling((maxY - minY) / gridSize);

            List<List<XYZ>> squares = new List<List<XYZ>>(cols * rows);

            for (int i = 0; i < cols; i++)
            {
                double x = minX + i * gridSize;
                double xNext = x + gridSize;

                for (int j = 0; j < rows; j++)
                {
                    double y = minY + j * gridSize;
                    double yNext = y + gridSize;

                    squares.Add(new List<XYZ>
                                {
                                    new XYZ(x, y, 0),
                                    new XYZ(xNext, y, 0),
                                    new XYZ(xNext, yNext, 0),
                                    new XYZ(x, yNext, 0)
                                });
                }
            }

            return squares;
        }

        /// <summary>
        /// Từ geometry của element, lấy ra danh sách các solid,mesh của element đó
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static (List<Solid> Solids, List<Mesh> Meshes) GetSolids(Element element, Document doc)
        {
            Options options = new Options();
            options.IncludeNonVisibleObjects = true;
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;
            GeometryElement elementGeo = element.get_Geometry(options);

            List<Solid> solids = new List<Solid>();
            List<Mesh> meshes = new List<Mesh>();

            foreach (GeometryObject geometryObj in elementGeo)
            {
                if (geometryObj is Solid solid)
                {
                    if (solid.Faces.Size > 0 && solid.Volume > 0)
                    {
                        solids.Add(solid);
                    }
                }
                if (geometryObj is GeometryInstance geomInstance)
                {
                    GeometryElement instanceGeometry = geomInstance.GetInstanceGeometry();
                    foreach (GeometryObject geometryObject in instanceGeometry)
                    {
                        if (geometryObject is Solid nestedSolid)
                        {
                            if (nestedSolid.Faces.Size > 0 && nestedSolid.Volume > 0)
                            {
                                solids.Add(nestedSolid);
                            }
                        }
                        if (geometryObject is Mesh mesh)
                        {
                            meshes.Add(mesh);
                        }
                    }
                }
            }
            return (solids, meshes);
        }

        /// <summary>
        /// Hàm dùng để lấy ra meshes của tất cả các faces
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="meshes"></param>
        /// <param name="faces"></param>
        public static void AddMesh<T>(List<Mesh> meshes, List<T> faces) where T : Face
        {
            foreach (var f in faces)
            {
                Mesh mesh = f.Triangulate();
                meshes.Add(mesh);
            }
        }

        /// <summary>
        /// So sánh 2 số double
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public static bool CompareDouble(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < CommonConstants.TOLERANCE;
        }

        #region Hàm dùng để nhóm các element sát nhau lại với nhau

        /// <summary>
        /// Hàm dùng để nhóm các element sát nhau lại với nhau
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elements"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static List<List<Element>> GroupElementsByBoundingBoxProximity(Document doc, List<Element> elements, double tolerance = 0.01)
        {
            var ungrouped = new HashSet<Element>(elements);
            var groups = new List<List<Element>>();

            while (ungrouped.Count > 0)
            {
                var startElement = ungrouped.First();
                ungrouped.Remove(startElement);

                var group = new List<Element> { startElement };
                var queue = new Queue<Element>();
                queue.Enqueue(startElement);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    var currentBox = current.get_BoundingBox(null);

                    if (currentBox == null)
                        continue;

                    var nearby = ungrouped
                        .Where(e => AreBoundingBoxesTouchingOrClose(currentBox, e.get_BoundingBox(null), tolerance))
                        .ToList();

                    foreach (var e in nearby)
                    {
                        group.Add(e);
                        queue.Enqueue(e);
                        ungrouped.Remove(e);
                    }
                }

                groups.Add(group);
            }

            return groups;
        }

        private static bool AreBoundingBoxesTouchingOrClose(BoundingBoxXYZ box1, BoundingBoxXYZ box2, double tolerance = CommonConstants.TOLERANCE * 10)
        {
            if (box1 == null || box2 == null)
                return false;

            XYZ min1 = box1.Min;
            XYZ max1 = box1.Max;
            XYZ min2 = box2.Min;
            XYZ max2 = box2.Max;

            // Nếu có giao nhau: return true
            bool overlapX = max1.X >= min2.X && min1.X <= max2.X;
            bool overlapY = max1.Y >= min2.Y && min1.Y <= max2.Y;
            bool overlapZ = max1.Z >= min2.Z && min1.Z <= max2.Z;

            if (overlapX && overlapY && overlapZ)
                return true;

            // Nếu không giao nhau, tính khoảng cách gần nhất
            double dx = Math.Max(0, Math.Max(min1.X - max2.X, min2.X - max1.X));
            double dy = Math.Max(0, Math.Max(min1.Y - max2.Y, min2.Y - max1.Y));
            double dz = Math.Max(0, Math.Max(min1.Z - max2.Z, min2.Z - max1.Z));

            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);

            return distance <= tolerance;
        }

        #endregion Hàm dùng để nhóm các element sát nhau lại với nhau

        /// <summary>
        /// Hàm dùng để tìm ra 2 điểm min và max của 1 element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static (XYZ min, XYZ max) GetElementBoundingBoxMinMax(Element element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            BoundingBoxXYZ bbox = element.get_BoundingBox(null);
            if (bbox == null)
                return (null, null);

            return (bbox.Min, bbox.Max);
        }
    }
}