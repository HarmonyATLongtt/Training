using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using FirstCommand.Support.Constants;

namespace FirstCommand.Support.GeometryHandle
{
    public static class GeometryUtility
    {
        //___Hàm kiểm tra xem hình vuông và hình tam giác có giao nhau không
        //___Start ___
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
        /// Hàm kiểm tra 2 vector có vuông góc không
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool AreVectorsPerpendicular(XYZ v1, XYZ v2)
        {
            if (v1.IsZeroLength() || v2.IsZeroLength())
                return false;  // Vector rỗng không có hướng xác định

            double dot = v1.Normalize().DotProduct(v2.Normalize());
            return Math.Abs(dot) < CommonConstants.COSINE_ANGLE_TOLERANCE_1_DEGREE;
        }

        /// <summary>
        /// Kiểm tra xem 1 vector có song song với 1 trục tọa độ nào không
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool IsPerpendicularToAxis(XYZ v, double axis)
        {
            //return Math.Abs(v.X) < TOLERANCE;
            return Math.Abs(axis) < CommonConstants.TOLERANCE;
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

        /// <summary>
        /// Hàm kiểm tra xem 2 vector có song song với nhau hay không
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool IsParallel(XYZ v1, XYZ v2, double tolerance)
        {
            var cross = v1.CrossProduct(v2);
            //return cross.GetLength() < COSINE_ANGLE_TOLERANCE_5_DEGREE;
            return cross.GetLength() < tolerance;
        }
    }
}