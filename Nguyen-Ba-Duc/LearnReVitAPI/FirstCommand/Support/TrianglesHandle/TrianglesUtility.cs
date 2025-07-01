using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FirstCommand.Support.GenericClass;
using FirstCommand.Support.GenericClass.ComparerClass;
using FirstCommand.Support.DrawOnRevit;
using FirstCommand.Support.PointHandle;
using FirstCommand.Support.LineHandle;
using FirstCommand.Support.Constants;
using FirstCommand.Support.GeometryHandle;

namespace FirstCommand.Support.TrianglesHandle
{
    public static class TrianglesUtility
    {
        /// <summary>
        /// Hàm vẽ ra 1 tam giác lớn nhất và nhỏ nhất trong 1 tập hợp các tam giác
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="doc"></param>
        public static void DrawHighestAndLowestTriangle(List<MeshTriangle> triangles, Document doc, bool isRevitLink, Transform transform)
        {
            double maxZ = double.MinValue;
            double minZ = double.MaxValue;
            MeshTriangle highestTri = null;
            MeshTriangle lowestTri = null;
            foreach (var tri in triangles)
            {
                var tupleValues = GetHighestAndLowestZPoint(tri);
                double highest = tupleValues.Max.Z;
                double lowest = tupleValues.Min.Z;

                if (highest > maxZ)
                {
                    maxZ = highest;
                    highestTri = tri;
                }
                if (lowest < minZ)
                {
                    minZ = lowest;
                    lowestTri = tri;
                }
            }

            List<XYZ> list1 = GetVerticesOfTriangle(lowestTri);
            List<XYZ> list2 = GetVerticesOfTriangle(highestTri);
            DrawLineFromTriangle(list1, doc, isRevitLink, transform);
            DrawLineFromTriangle(list2, doc, isRevitLink, transform);
        }

        /// <summary>
        /// Hàm vẽ line từ 3 cạnh của 1 tam giác
        /// </summary>
        /// <param name="points"></param>
        /// <param name="doc"></param>
        public static void DrawLineFromTriangle(List<XYZ> points, Document doc, bool isRevitLink, Transform transform)
        {
            if (points.Count == 3)
            {
                XYZ p1 = points[0];
                XYZ p2 = points[1];
                XYZ p3 = points[2];
                DrawPointLineArc.CreateModelLine(doc, p1, p2, isRevitLink, transform, 0);
                DrawPointLineArc.CreateModelLine(doc, p2, p3, isRevitLink, transform, 0);
                DrawPointLineArc.CreateModelLine(doc, p3, p1, isRevitLink, transform, 0);
            }
        }

        /// <summary>
        /// Lấy ra triangle có điểm thấp nhất
        /// </summary>
        /// <param name="tri1"></param>
        /// <param name="tri2"></param>
        /// <returns></returns>
        public static bool CompareTwoTriangles(double tri1TopZ, MeshTriangle tri2)
        {
            if (tri1TopZ >= GetHighestAndLowestZPoint(tri2).Max.Z)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Hàm tạo line từ các cạnh của triangle
        /// </summary>
        /// <param name="tri"></param>
        /// <returns></returns>
        public static List<Line> GetTriangleEdges(MeshTriangle tri)
        {
            // Set Z=0 để đưa các điểm về mặt phẳng XY để so sánh với các hình vuông trên XY
            var a = PointUtility.SetPointWithNewZValue(tri.get_Vertex(0), 0);
            var b = PointUtility.SetPointWithNewZValue(tri.get_Vertex(1), 0);
            var c = PointUtility.SetPointWithNewZValue(tri.get_Vertex(2), 0);

            List<Line> lines = new List<Line>();

            LineUtility.CreateLineFromTriangleEdges(a, b, lines);
            LineUtility.CreateLineFromTriangleEdges(b, c, lines);
            LineUtility.CreateLineFromTriangleEdges(c, a, lines);
            return lines;
        }

        /// <summary>
        /// Hàm kiểm tra xem 2 tam giác khi cho đồng phẳng có giao nhau hay không
        /// </summary>
        /// <param name="tri1"></param>
        /// <param name="tri2"></param>
        /// <returns></returns>
        public static bool IsTrianglesIntersect(List<Line> edges1, MeshTriangle tri2)
        {
            //List<Line> edges1 = GetTriangleEdges(tri1);
            List<Line> edges2 = GetTriangleEdges(tri2);

            foreach (var l1 in edges1)
            {
                foreach (var l2 in edges2)
                {
                    IntersectionResultArray results;
                    SetComparisonResult comparisonResult = l1.Intersect(l2, out results);
                    if (comparisonResult == SetComparisonResult.Overlap && results != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Hàm lấy ra tập hợp các tam giác cùng phương với 1 line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static List<MeshTriangle> GetTrianglesParallelToLine(Line line, List<MeshTriangle> triangles)
        {
            List<MeshTriangle> result = new List<MeshTriangle>();
            XYZ normalizedDirection = line.Direction.Normalize();
            foreach (var triangle in triangles)
            {
                XYZ normal = GetNormalFromTriangle(triangle);

                // Nếu normal vuông góc với direction thì dot product gần 0
                double dot = normal.Normalize().DotProduct(normalizedDirection);

                //if (Math.Abs(dot) < COSINE_ANGLE_TOLERANCE_5_DEGREE)
                if (Math.Abs(dot) < CommonConstants.COSINE_ANGLE_TOLERANCE_1_DEGREE)
                {
                    result.Add(triangle);
                }
            }
            return result;
        }

        /// <summary>
        /// Hàm dùng để kiểm tra xem trong 1 tam giác có bất kỳ cạnh nào có độ dài lớn
        /// hơn 1 length cho trước
        /// </summary>
        /// <param name="triangle"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool HasEdgeLongerThan(MeshTriangle triangle, double length)
        {
            List<Line> edges = GetTriangleEdges(triangle);
            return edges.Any(e => e.Length > length);
        }

        /// <summary>
        /// Hàm lấy ra điểm thấp nhất và cao nhất của 1 tam giác
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public static (XYZ Max, XYZ Min) GetHighestAndLowestZPoint(MeshTriangle triangle)
        {
            XYZ v0 = triangle.get_Vertex(0);
            XYZ v1 = triangle.get_Vertex(1);
            XYZ v2 = triangle.get_Vertex(2);

            XYZ highest = v0;
            XYZ lowest = v0;

            if (v1.Z > highest.Z) highest = v1;
            if (v1.Z < lowest.Z) lowest = v1;

            if (v2.Z > highest.Z) highest = v2;
            if (v2.Z < lowest.Z) lowest = v2;

            return (highest, lowest);
        }

        /// <summary>
        /// Hàm lấy ra vector normal của 1 tam giác
        /// </summary>
        /// <param name="tri"></param>
        /// <returns></returns>
        public static XYZ GetNormalFromTriangle(MeshTriangle tri)
        {
            XYZ p1 = tri.get_Vertex(0);
            XYZ p2 = tri.get_Vertex(1);
            XYZ p3 = tri.get_Vertex(2);

            XYZ edge1 = p2 - p1;
            XYZ edge2 = p3 - p1;

            XYZ normal = edge1.CrossProduct(edge2).Normalize();
            if (normal.IsZeroLength())
            {
                TaskDialog.Show("Noti", "Tam giác gần như phẳng");
                return null;
            }
            return normal;
        }

        /// <summary>
        /// Hàm dùng để gom nhóm các tam giác có chung đỉnh lại với nhau
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static List<List<MeshTriangle>> GroupMeshTrianglesBySharedVertices(HashSet<MeshTriangle> triangles)
        {
            var result = new List<List<MeshTriangle>>();
            var visited = new HashSet<MeshTriangle>();
            var vertexToTriangles = new Dictionary<XYZ, List<MeshTriangle>>(new XYZComparer(CommonConstants.TOLERANCE));

            // Bước 1: tạo từ điển tra nhanh các đỉnh → các tam giác chứa đỉnh đó
            foreach (var triangle in triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    var vertex = triangle.get_Vertex(i);

                    if (!vertexToTriangles.TryGetValue(vertex, out var list))
                    {
                        list = new List<MeshTriangle>();
                        vertexToTriangles[vertex] = list;
                    }

                    list.Add(triangle);
                }
            }

            // Bước 2: gom nhóm tam giác có chung đỉnh
            foreach (var triangle in triangles)
            {
                if (visited.Contains(triangle))
                    continue;

                var group = new List<MeshTriangle>();
                var toCheck = new List<MeshTriangle> { triangle };

                for (int i = 0; i < toCheck.Count; i++)
                {
                    var current = toCheck[i];
                    if (visited.Contains(current))
                        continue;

                    visited.Add(current);
                    group.Add(current);

                    // Tìm các tam giác khác có đỉnh trùng (gần) với current
                    for (int j = 0; j < 3; j++)
                    {
                        var vertex = current.get_Vertex(j);

                        if (!vertexToTriangles.TryGetValue(vertex, out var neighbors))
                            continue;

                        foreach (var neighbor in neighbors)
                        {
                            if (!visited.Contains(neighbor) && !toCheck.Contains(neighbor))
                            {
                                toCheck.Add(neighbor);
                            }
                        }
                    }
                }

                result.Add(group);
            }

            return result;
        }

        /// <summary>
        /// Hàm kiểm tra xem 2 tam giác có điểm chung không
        /// </summary>
        /// <param name="vertsA"></param>
        /// <param name="vertsB"></param>
        /// <returns></returns>
        public static bool HasCommonVertex(List<XYZ> vertsA, List<XYZ> vertsB)
        {
            return vertsA.Any(a => vertsB.Any(b => a.IsAlmostEqualTo(b, CommonConstants.TOLERANCE)));

            //foreach (var a in vertsA)
            //{
            //    foreach (var b in vertsB)
            //    {
            //        if (a.IsAlmostEqualTo(b, TOLERANCE))
            //            return true;
            //    }
            //}
            //return false;
        }

        /// <summary>
        /// Hàm dùng để lấy ra danh sách các điểm thuộc 1 triangle
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public static List<XYZ> GetVerticesOfTriangle(MeshTriangle triangle)
        {
            return new List<XYZ>
            {
                triangle.get_Vertex(0),
                triangle.get_Vertex(1),
                triangle.get_Vertex(2)
            };
        }

        /// <summary>
        /// Hàm dùng để lấy ra các cặp điểm trong tam giác
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public static List<(XYZ, XYZ)> GetXYZPairsOfTriangle(MeshTriangle triangle)
        {
            return new List<(XYZ, XYZ)>
            {
                (triangle.get_Vertex(0), triangle.get_Vertex(1)),
                (triangle.get_Vertex(1), triangle.get_Vertex(2)),
                (triangle.get_Vertex(2), triangle.get_Vertex(0))
            };
        }

        /// <summary>
        /// Hàm dùng để lấy ra tất cả tam giác trong 1 solid
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static List<MeshTriangle> ExtractTrianglesFromSolid(Solid solid)
        {
            List<MeshTriangle> triangles = new List<MeshTriangle>();
            foreach (Face face in solid.Faces)
            {
                Mesh mesh = face.Triangulate();
                int triCount = mesh.NumTriangles;

                for (int i = 0; i < triCount; i++)
                {
                    MeshTriangle tri = mesh.get_Triangle(i);
                    triangles.Add(tri);
                }
            }
            return triangles;
        }

        /// <summary>
        /// Hàm dùng để gom nhóm các tam giác có điểm chung và cùng nằm trên 1 mặt phẳng
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static List<List<MeshTriangle>> GroupTrianglesByVertexAndNormal(List<MeshTriangle> triangles)
        {
            // B1: Tạo dictionary: mỗi điểm XYZ ánh xạ đến các tam giác chứa nó
            var vertexToTriangles = new Dictionary<XYZ, List<MeshTriangle>>(new XYZComparer());

            foreach (var tri in triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    XYZ v = tri.get_Vertex(i);
                    if (!vertexToTriangles.TryGetValue(v, out var list))
                    {
                        list = new List<MeshTriangle>();
                        vertexToTriangles[v] = list;
                    }
                    list.Add(tri);
                }
            }

            // B2: Gom nhóm các tam giác
            var result = new List<List<MeshTriangle>>();
            var visited = new HashSet<MeshTriangle>();

            foreach (var tri in triangles)
            {
                if (visited.Contains(tri)) continue;

                var group = new List<MeshTriangle>();
                var queue = new Queue<MeshTriangle>();
                queue.Enqueue(tri);
                visited.Add(tri);

                var normal = GetNormalFromTriangle(tri);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    group.Add(current);

                    for (int i = 0; i < 3; i++)
                    {
                        XYZ v = current.get_Vertex(i);
                        if (!vertexToTriangles.TryGetValue(v, out var candidates)) continue;

                        foreach (var neighbor in candidates)
                        {
                            if (visited.Contains(neighbor)) continue;

                            var neighborNormal = GetNormalFromTriangle(neighbor);
                            if (GeometryUtility.IsParallel(normal, neighborNormal, CommonConstants.TOLERANCE))
                            {
                                queue.Enqueue(neighbor);
                                visited.Add(neighbor);
                            }
                        }
                    }
                }

                result.Add(group);
            }

            return result;
        }
    }
}