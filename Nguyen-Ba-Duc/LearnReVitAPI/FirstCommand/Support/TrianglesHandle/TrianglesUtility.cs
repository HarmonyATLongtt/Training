using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.UI;
using FirstCommand.Support.Constants;
using FirstCommand.Support.DrawOnRevit;

using FirstCommand.Support.GenericClass.ComparerUtils;
using FirstCommand.Support.GeometryHandle;
using FirstCommand.Support.LineHandle;
using FirstCommand.Support.PointHandle;
using FirstCommand.Support.VectorHandle;
using Microsoft.SqlServer.Server;

namespace FirstCommand.Support.TrianglesHandle
{
    public static class TrianglesUtility
    {
        /// <summary>
        /// Hàm vẽ ra 1 tam giác lớn nhất và nhỏ nhất trong 1 tập hợp các tam giác
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="doc"></param>
        public static void DrawHighestAndLowestTriangle(List<MeshTriangle> triangles, Document doc, Transform transform)
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

            List<XYZ> list1 = GetVerticesOfTriangles(triangle: lowestTri);
            List<XYZ> list2 = GetVerticesOfTriangles(triangle: highestTri);
            DrawLineFromTriangle(list1, doc, transform);
            DrawLineFromTriangle(list2, doc, transform);
        }

        /// <summary>
        /// Hàm vẽ line từ 3 cạnh của 1 tam giác
        /// </summary>
        /// <param name="points"></param>
        /// <param name="doc"></param>
        public static void DrawLineFromTriangle(List<XYZ> points, Document doc, Transform trans)
        {
            if (points.Count == 3)
            {
                XYZ p1 = points[0];
                XYZ p2 = points[1];
                XYZ p3 = points[2];
                DrawPointLineArc.CreateModelLine(doc, p1, p2, transform: trans);
                DrawPointLineArc.CreateModelLine(doc, p2, p3, transform: trans);
                DrawPointLineArc.CreateModelLine(doc, p3, p1, transform: trans);
            }
        }

        /// <summary>
        /// Hàm dùng để vẽ tam giác
        /// </summary>
        /// <param name="meshTriangles"></param>
        /// <param name="doc"></param>
        /// <param name="isRevitLink"></param>
        /// <param name="transform"></param>
        public static void DrawTriangles(List<MeshTriangle> meshTriangles, Document doc, Transform trans)
        {
            foreach (var tri in meshTriangles)
            {
                var points = GetVerticesOfTriangles(triangle: tri);
                if (points.Count == 3)
                {
                    XYZ p1 = points[0];
                    XYZ p2 = points[1];
                    XYZ p3 = points[2];
                    DrawPointLineArc.CreateModelLine(doc, p1, p2, transform: trans);
                    DrawPointLineArc.CreateModelLine(doc, p2, p3, transform: trans);
                    DrawPointLineArc.CreateModelLine(doc, p3, p1, transform: trans);
                }
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
        /// Hàm lấy ra tập hợp các tam giác song song với 1 vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static List<MeshTriangle> GetTrianglesParallelToVector(XYZ vector, List<MeshTriangle> triangles, double tolerance)
        {
            List<MeshTriangle> result = new List<MeshTriangle>();
            //XYZ normalizedDirection = line.Direction.Normalize();
            foreach (var triangle in triangles)
            {
                XYZ normal = GetNormalFromTriangle(triangle);

                // Nếu normal vuông góc với direction thì dot product gần 0
                double dot = normal.Normalize().DotProduct(vector);

                //if (Math.Abs(dot) < COSINE_ANGLE_TOLERANCE_5_DEGREE)
                if (Math.Abs(dot) < tolerance)
                {
                    result.Add(triangle);
                }
            }
            return result;
        }

        /// <summary>
        /// Lấy ra tất cả tam giác vuông góc với 1 vector
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static List<MeshTriangle> GetTrianglesPerpendicularToVector(XYZ vector, List<MeshTriangle> triangles, double tolerance)
        {
            List<MeshTriangle> result = new List<MeshTriangle>();
            foreach (var triangle in triangles)
            {
                XYZ normal = GetNormalFromTriangle(triangle);

                if (VectorUtility.AreParallel(vector, normal, tolerance))
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
            XYZ v0 = triangle.get_Vertex(0);
            XYZ v1 = triangle.get_Vertex(1);
            XYZ v2 = triangle.get_Vertex(2);

            if ((v0 - v1).GetLength() > length) return true;
            if ((v1 - v2).GetLength() > length) return true;
            if ((v2 - v0).GetLength() > length) return true;

            return false;
            //List<Line> edges = GetTriangleEdges(triangle);
            //return edges.Any(e => e.Length > length);
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

            XYZ normal = edge1.CrossProduct(edge2);

            return VectorUtility.NormalizeSafe(normal);

            //XYZ normal = edge1.CrossProduct(edge2).Normalize();
            //if (normal.IsZeroLength())
            //{
            //    TaskDialog.Show("Noti", "Tam giác gần như phẳng");
            //    return null;
            //}
            //return normal;
        }

        /// <summary>
        /// Hàm dùng để gom nhóm các tam giác có chung đỉnh lại với nhau
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static List<List<MeshTriangle>> GroupMeshTrianglesBySharedVertices(TriangleVertexMap data, List<MeshTriangle> triangles)
        {
            var result = new List<List<MeshTriangle>>();
            var visited = new HashSet<MeshTriangle>();
            //var vertexToTriangles = new Dictionary<XYZ, List<MeshTriangle>>(Comparers.XYZ);

            //// Bước 1: tạo từ điển tra nhanh các đỉnh → các tam giác chứa đỉnh đó
            //foreach (var triangle in triangles)
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        var vertex = triangle.get_Vertex(i);

            //        if (!vertexToTriangles.TryGetValue(vertex, out var list))
            //        {
            //            list = new List<MeshTriangle>();
            //            vertexToTriangles[vertex] = list;
            //        }

            //        list.Add(triangle);
            //    }
            //}

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

                        if (!data.VertexToTriangles.TryGetValue(vertex, out var neighbors))
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
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool HasCommonVertex(MeshTriangle a, MeshTriangle b)
        {
            var vertsA = TrianglesUtility.GetVerticesOfTriangles(triangle: a);
            var vertsB = TrianglesUtility.GetVerticesOfTriangles(triangle: b);
            return vertsA.Any(va => vertsB.Any(vb => va.IsAlmostEqualTo(vb, CommonConstants.TOLERANCE)));
        }

        /// <summary>
        ///  Hàm dùng để lấy ra tất cả các điểm trong 1 tập hợp tất cả tam giác
        /// </summary>
        /// <param name="meshTriangles"></param>
        /// <param name="triangle"></param>
        /// <returns></returns>
        public static List<XYZ> GetVerticesOfTriangles(List<MeshTriangle> meshTriangles = null, MeshTriangle triangle = null)
        {
            if (triangle != null)
            {
                return new List<XYZ>
                {
                    triangle.get_Vertex(0),
                    triangle.get_Vertex(1),
                    triangle.get_Vertex(2)
                };
            }
            else if (meshTriangles != null)
            {
                var points = new HashSet<XYZ>(Comparers.XYZ);
                foreach (var tri in meshTriangles)
                {
                    points.Add(tri.get_Vertex(0));
                    points.Add(tri.get_Vertex(1));
                    points.Add(tri.get_Vertex(2));
                }
                return points.ToList();
            }
            return null;
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
            List<Face> faces = new List<Face>();
            foreach (Face face in solid.Faces)
            {
                faces.Add(face);
            }
            triangles.AddRange(ExtractTrianglesFromFaces(faces));
            //foreach (Face face in solid.Faces)
            //{
            //    Mesh mesh = face.Triangulate();
            //    //int triCount = mesh.NumTriangles;

            //    //for (int i = 0; i < triCount; i++)
            //    //{
            //    //    MeshTriangle tri = mesh.get_Triangle(i);
            //    //    triangles.Add(tri);
            //    //}
            //}
            return triangles;
        }

        /// <summary>
        /// Hàm dùng để lấy ra tất cả tam giác trong tập hợp các face
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="faces"></param>
        /// <returns></returns>
        public static List<MeshTriangle> ExtractTrianglesFromFaces<T>(List<T> faces) where T : Face
        {
            List<MeshTriangle> triangles = new List<MeshTriangle>();
            foreach (Face face in faces)
            {
                Mesh mesh = null;
                if (face is PlanarFace pf)
                {
                    mesh = pf.Triangulate();
                }
                else if (face is CylindricalFace cf)
                {
                    mesh = cf.Triangulate();
                }
                triangles.AddRange(ExtractTrianglesFromMeshes(mesh));
            }
            return triangles;
        }

        /// <summary>
        /// Hàm dùng để gom nhóm các tam giác có điểm chung và cùng nằm trên 1 mặt phẳng
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static List<List<MeshTriangle>> GroupTrianglesByVertexAndNormal(List<MeshTriangle> triangles, double numOfTriangleMakePlanarFace)
        {
            // B1: Tạo dictionary: mỗi điểm XYZ ánh xạ đến các tam giác chứa nó
            var vertexToTriangles = new Dictionary<XYZ, List<MeshTriangle>>(Comparers.XYZ);

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
                            if (VectorUtility.AreParallel(normal, neighborNormal, CommonConstants.TOLERANCE))
                            {
                                queue.Enqueue(neighbor);
                                visited.Add(neighbor);
                            }
                        }
                    }
                }

                if (group.Count >= numOfTriangleMakePlanarFace)
                {
                    result.Add(group);
                }
            }

            return result;
        }

        /// <summary>
        /// Tạo dictionary: mỗi điểm XYZ ánh xạ đến các tam giác chứa nó
        /// </summary>
        /// <param name="meshTriangles"></param>
        /// <returns></returns>
        public static Dictionary<XYZ, List<MeshTriangle>> VertexToTrianglesMapping(List<MeshTriangle> meshTriangles)
        {
            var vertexToTriangles = new Dictionary<XYZ, List<MeshTriangle>>(Comparers.XYZ);

            foreach (var tri in meshTriangles)
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
            return vertexToTriangles;
        }

        /// <summary>
        /// Hàm dùng để gom nhóm các tam giác có điểm chung từ 1 triangle cho trước
        /// </summary>
        /// <param name="inputTriangles"></param>
        /// <param name="startTriangle"></param>
        /// <returns></returns>
        public static List<MeshTriangle> FindConnectedTrianglesByVertex(List<MeshTriangle> inputTriangles, MeshTriangle startTriangle)
        {
            var visited = new HashSet<MeshTriangle>();
            var result = new List<MeshTriangle>();
            var queue = new Queue<MeshTriangle>();

            // Tạo dictionary ánh xạ từ XYZ → các tam giác chứa nó
            var vertexToTriangles = new Dictionary<XYZ, List<MeshTriangle>>(Comparers.XYZ);

            foreach (var tri in inputTriangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    var v = tri.get_Vertex(i);
                    if (!vertexToTriangles.TryGetValue(v, out var list))
                    {
                        list = new List<MeshTriangle>();
                        vertexToTriangles[v] = list;
                    }
                    list.Add(tri);
                }
            }

            // Bắt đầu BFS
            queue.Enqueue(startTriangle);
            visited.Add(startTriangle);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                for (int i = 0; i < 3; i++)
                {
                    var v = current.get_Vertex(i);
                    if (!vertexToTriangles.TryGetValue(v, out var neighbors)) continue;

                    foreach (var neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Hàm dùng để lấy ra boundingboxXYZ của 1 tập hợp các meshtriangles
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static (XYZ Min, XYZ Max) GetMinMaxXYZFromMeshTriangles(List<MeshTriangle> triangles)
        {
            if (triangles == null || triangles.Count == 0)
                return (null, null);

            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

            foreach (var tri in triangles)
            {
                for (int i = 0; i < 3; i++)
                {
                    var pt = tri.get_Vertex(i);
                    minX = Math.Min(minX, pt.X);
                    minY = Math.Min(minY, pt.Y);
                    minZ = Math.Min(minZ, pt.Z);

                    maxX = Math.Max(maxX, pt.X);
                    maxY = Math.Max(maxY, pt.Y);
                    maxZ = Math.Max(maxZ, pt.Z);
                }
            }

            XYZ minPoint = new XYZ(minX, minY, minZ);
            XYZ maxPoint = new XYZ(maxX, maxY, maxZ);
            return (minPoint, maxPoint);
        }

        /// <summary>
        /// Hàm dùng để lấy ra khoảng cách lớn nhất của 1 tập hợp các meshtriangles
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static double GetMaxLengthOfGroupMeshTriangles(List<MeshTriangle> triangles)
        {
            var (minPoint, maxPoint) = GetMinMaxXYZFromMeshTriangles(triangles);
            return minPoint.DistanceTo(maxPoint);
        }

        /// <summary>
        /// Hàm dùng để gom nhóm các tam giác có chung 2 đỉnh lại với nhau
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static List<List<MeshTriangle>> GroupMeshTrianglesBySharedTwoVertices(HashSet<MeshTriangle> triangles)
        {
            var result = new List<List<MeshTriangle>>();
            var visited = new HashSet<MeshTriangle>();
            //var vertexToTriangles = new Dictionary<XYZ, List<MeshTriangle>>(new XYZComparer(CommonConstants.TOLERANCE));

            // Bước 1: tạo từ điển tra nhanh các đỉnh → các tam giác chứa đỉnh đó
            var vertexToTriangles = new Dictionary<UnorderedXYZPair, List<MeshTriangle>>();
            foreach (var tri in triangles)
            {
                var XYZPairs = GetXYZPairsOfTriangle(tri);
                foreach (var pair in XYZPairs)
                {
                    var newPair = new UnorderedXYZPair(pair.Item1, pair.Item2);
                    if (!vertexToTriangles.TryGetValue(newPair, out var list))
                    {
                        list = new List<MeshTriangle>();
                        vertexToTriangles[newPair] = list;
                    }

                    list.Add(tri);
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
                    var XYZPairs = GetXYZPairsOfTriangle(current);
                    foreach (var pair in XYZPairs)
                    {
                        var newPair = new UnorderedXYZPair(pair.Item1, pair.Item2);
                        if (!vertexToTriangles.TryGetValue(newPair, out var neighbors))
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
        /// Hàm dùng để gom nhóm các tam giác có 2 đỉnh chung, với đầu vào là 1 tam giác cho trước
        /// </summary>
        /// <param name="inputTriangles"></param>
        /// <param name="startTriangle"></param>
        /// <returns></returns>
        public static List<MeshTriangle> FindConnectedTrianglesBySharedTwoVertex(List<MeshTriangle> inputTriangles, MeshTriangle startTriangle)
        {
            var visited = new HashSet<MeshTriangle>();
            var result = new List<MeshTriangle>();
            var queue = new Queue<MeshTriangle>();

            // Tạo dictionary ánh xạ từ XYZ → các tam giác chứa nó
            var vertexToTriangles = new Dictionary<UnorderedXYZPair, List<MeshTriangle>>();
            foreach (var tri in inputTriangles)
            {
                var XYZPairs = GetXYZPairsOfTriangle(tri);
                foreach (var pair in XYZPairs)
                {
                    var newPair = new UnorderedXYZPair(pair.Item1, pair.Item2);
                    if (!vertexToTriangles.TryGetValue(newPair, out var list))
                    {
                        list = new List<MeshTriangle>();
                        vertexToTriangles[newPair] = list;
                    }

                    list.Add(tri);
                }
            }

            // Bắt đầu BFS
            queue.Enqueue(startTriangle);
            visited.Add(startTriangle);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                var XYZPairs = GetXYZPairsOfTriangle(current);
                foreach (var pair in XYZPairs)
                {
                    var newPair = new UnorderedXYZPair(pair.Item1, pair.Item2);
                    if (!vertexToTriangles.TryGetValue(newPair, out var neighbors)) continue;

                    foreach (var neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Hàm dùng để lấy ra tất cả meshtriangle trong mesh
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        public static List<MeshTriangle> ExtractTrianglesFromMeshes(Mesh mesh)
        {
            List<MeshTriangle> triangles = new List<MeshTriangle>();

            int triCount = mesh.NumTriangles;

            for (int i = 0; i < triCount; i++)
            {
                MeshTriangle tri = mesh.get_Triangle(i);
                triangles.Add(tri);
            }

            return triangles;
        }

        /// <summary>
        /// Hàm dùng để lấy ra giá trị min max X và Y của 1 triangle
        /// </summary>
        /// <param name="tri"></param>
        /// <param name="minX"></param>
        /// <param name="minY"></param>
        /// <param name="maxX"></param>
        /// <param name="maxY"></param>
        public static void GetMinMaxXYOfTriangle(MeshTriangle tri, out double minX, out double minY, out double maxX, out double maxY)
        {
            XYZ p1 = tri.get_Vertex(0);
            XYZ p2 = tri.get_Vertex(1);
            XYZ p3 = tri.get_Vertex(2);

            minX = Math.Min(p1.X, Math.Min(p2.X, p3.X));
            minY = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
            maxX = Math.Max(p1.X, Math.Max(p2.X, p3.X));
            maxY = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
        }

        /// <summary>
        /// Hàm dùng để ánh xạ BoundingboxXY của tam giác vào các grid
        /// </summary>
        /// <param name="meshTriangles"></param>
        /// <param name="gridSize"></param>
        /// <returns></returns>
        public static Dictionary<(int, int), List<MeshTriangle>> SpatialHashMeshTrianglesToGrid(List<MeshTriangle> meshTriangles, double gridSize)

        {
            var cellToTriangles = new Dictionary<(int, int), List<MeshTriangle>>();

            foreach (var tri in meshTriangles)
            {
                GetMinMaxXYOfTriangle(tri, out double minX, out double minY, out double maxX, out double maxY);

                var (minI, minJ) = PointUtility.GetGridIndex(new XYZ(minX, minY, 0), gridSize);
                var (maxI, maxJ) = PointUtility.GetGridIndex(new XYZ(maxX, maxY, 0), gridSize);

                for (int i = minI; i <= maxI; i++)
                {
                    for (int j = minJ; j <= maxJ; j++)
                    {
                        var key = (i, j);
                        if (!cellToTriangles.ContainsKey(key))
                            cellToTriangles[key] = new List<MeshTriangle>();

                        cellToTriangles[key].Add(tri); // Gán tam giác này vào cell (i, j)
                    }
                }
            }
            return cellToTriangles;
        }

        /// <summary>
        /// Danh sách map mỗi triangle với danh sách các điểm của nó
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        public static Dictionary<MeshTriangle, List<XYZ>> GetTriangleVerticesDict(List<MeshTriangle> triangles)
        {
            var triangleVertexDict = new Dictionary<MeshTriangle, List<XYZ>>();

            foreach (var tri in triangles)
            {
                if (!triangleVertexDict.ContainsKey(tri))
                {
                    triangleVertexDict[tri] = new List<XYZ>
                        {
                            tri.get_Vertex(0),
                            tri.get_Vertex(1),
                            tri.get_Vertex(2)
                        };
                }
            }
            return triangleVertexDict;
        }
    }
}