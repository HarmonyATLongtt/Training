using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using FirstCommand.Support;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class RailingAnalyzer : IExternalCommand
    {
        private const double tolerance = 1e-6;
        private const double cosineAngleTolerance = 0.0872; // 5 độ (góc lệch cho phép để vector normal và trục Z được coi là vuông góc)
        private Transform transform = null;
        private double defaultRadius = 0;
        private double minX = 0;
        private double minY = 0;
        private double maxX = 0;
        private double maxY = 0;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                //IList<Reference> selectedRefs = uidoc.Selection.PickObjects(ObjectType.Element, "Chọn các đối tượng");
                IList<Reference> selectedRefs = uidoc.Selection.PickObjects(ObjectType.LinkedElement, "Chọn các đối tượng");

                if (selectedRefs.Count == 0)
                {
                    TaskDialog.Show("Thông báo", "Không có đối tượng nào được chọn.");
                    return Result.Cancelled;
                }
                List<CylinderInfo> cylinderInfosOfElements = new List<CylinderInfo>();
                List<Mesh> meshes = new List<Mesh>();
                foreach (Reference r in selectedRefs)
                {
                    //ElementId elementId = r.ElementId;
                    //Element element = doc.GetElement(elementId);

                    Element linkInstance = uidoc.Document.GetElement(r.ElementId);
                    RevitLinkInstance rli = linkInstance as RevitLinkInstance;

                    Document linkDoc = rli.GetLinkDocument();

                    ElementId linkedElemId = r.LinkedElementId;

                    Element linkedElem = linkDoc.GetElement(linkedElemId);
                    transform = rli.GetTransform();

                    var (minPoint, maxPoint) = GeometryUtility.GetBoundingBoxExtents(linkedElem);
                    if (minPoint != null && maxPoint != null)
                    {
                        minX = minPoint.X;
                        maxX = maxPoint.X;
                        minY = minPoint.Y;
                        maxY = maxPoint.Y;
                    }

                    var tupleValues = GetCylinderInfosFromElements(linkedElem, doc);
                    List<CylinderInfo> cylinderInfos = tupleValues.CylinderInfos;
                    meshes.AddRange(tupleValues.Meshs);

                    // Trường hợp model là 1 khối thống nhất
                    //if (cylinderInfos.Count > 1 && cylinderInfos.Count != 2)
                    if (cylinderInfos.Count > 1)
                    {
                        PrepareDataForExecution(cylinderInfos, doc, uidoc, meshes);
                    }
                    else if (cylinderInfos.Count == 1)
                    {
                        CylinderInfo firstCylinderInfo = cylinderInfos.FirstOrDefault();

                        cylinderInfosOfElements.Add(firstCylinderInfo);
                    }
                    // trường hợp chỉ có 2 trụ
                    //else if (cylinderInfos.Count == 2)
                    //{
                    //    HandleInCaseHaveTwoCylinderInfos(doc, cylinderInfos);
                    //}
                }
                // Trường hợp model gồm nhiều element ghép lại
                if (cylinderInfosOfElements.Count > 0 && cylinderInfosOfElements.Count != 2)
                {
                    PrepareDataForExecution(cylinderInfosOfElements, doc, uidoc, meshes);
                }
                // trường hợp chỉ có 2 trụ
                //else if (cylinderInfosOfElements.Count == 2)
                //{
                //    HandleInCaseHaveTwoCylinderInfos(doc, cylinderInfosOfElements);
                //}
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Người dùng nhấn ESC
                TaskDialog.Show("Thông báo", "Command bị hủy bởi người dùng.");
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }

        private void TestFunction(List<MeshTriangle> triangles, Document doc)
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
            DrawLineFromTriangle(list1, doc);
            DrawLineFromTriangle(list2, doc);
        }

        private void DrawLineFromTriangle(List<XYZ> points, Document doc)
        {
            if (points.Count == 3)
            {
                XYZ p1 = points[0];
                XYZ p2 = points[1];
                XYZ p3 = points[2];
                CreateModelLine(doc, p1, p2);
                CreateModelLine(doc, p2, p3);
                CreateModelLine(doc, p3, p1);
            }
        }

        private void HandleInCaseHaveTwoCylinderInfos(Document doc, List<CylinderInfo> cylinderInfos)
        {
            if (cylinderInfos.Count == 2)
            {
                XYZ firstPoint = cylinderInfos[0].TopPoint;
                XYZ endPoint = cylinderInfos[1].TopPoint;
                double length = firstPoint.DistanceTo(endPoint);
                CreateModelLine(doc, firstPoint, endPoint);
                TaskDialog.Show("Nofi", "The length of railing is : " + length.ToString());
            }
        }

        private void PrepareDataForExecution(List<CylinderInfo> cylinderInfos, Document doc, UIDocument uiDoc, List<Mesh> meshes)
        {
            var mergedList = MergeCylinderInfos(cylinderInfos);
            SortInstancesAlongLine(mergedList);
            if (mergedList.Count == 2)
            {
                GetTopViewShape(meshes, mergedList, doc);
            }
            else if (GroupPointsOnSamePlane(mergedList).Count == 2)
            {
                bool arePointsColinear = true;
                for (int i = 0; i < mergedList.Count - 1; i++)
                {
                    if (!ArePointsColinear(mergedList[i], mergedList[i + 1], mergedList[i].Radius))
                    {
                        arePointsColinear = false;
                        break;
                    }
                }
                if (arePointsColinear)
                {
                    SetSameZForAllCylinderInfos(mergedList);
                    GetTopViewShape(meshes, mergedList, doc);
                }
                else
                {
                    GetTopViewShape(meshes, mergedList, doc);
                    //var results = new List<List<CylinderInfo>> { mergedList };
                    //CalculateSpiralRailingLength(results, doc, uiDoc);
                }
            }
            else
            {
                var result = GroupPointsForDistanceCalculation(mergedList);
                double length = 0;
                foreach (var group in result)
                {
                    XYZ firstPoint = group.FirstOrDefault().TopPoint;
                    XYZ endPoint = group.LastOrDefault().TopPoint;
                    length += firstPoint.DistanceTo(endPoint);
                    CreateModelLine(doc, firstPoint, endPoint);
                }
                TaskDialog.Show("Nofi", "The length of railing is : " + length.ToString());
            }
        }

        /// <summary>
        /// Trường hợp tất cả các trụ được coi là bằng nhau thì để tránh sai số, set lại Z cho tất cả bằng nhau
        /// </summary>
        /// <param name="cylinderInfos"> Danh sách các trụ</param>
        private void SetSameZForAllCylinderInfos(List<CylinderInfo> cylinderInfos)
        {
            double minZ = double.MaxValue;
            foreach (var c in cylinderInfos)
            {
                if (minZ > c.TopPoint.Z)
                {
                    minZ = c.TopPoint.Z;
                }
            }
            foreach (var c in cylinderInfos)
            {
                c.TopPoint = SetOriginPoint(c.TopPoint, minZ);
            }
        }

        private void GetTopViewShape(List<Mesh> meshes, List<CylinderInfo> cylinderInfos, Document doc)
        {
            //if (cylinderInfos.Count > 2)
            //{
            //CylinderInfo firstCylinderInfo = cylinderInfos.First();
            CylinderInfo firstCylinderInfo = cylinderInfos[5];
            double radius = firstCylinderInfo.Radius;
            CylinderInfo lastCylinderInfo = cylinderInfos.Last();
            XYZ firstPoint = firstCylinderInfo.TopPoint;
            XYZ lastPoint = lastCylinderInfo.TopPoint;

            // Lấy ra tất cả các triangles của các meshes
            List<MeshTriangle> trianglesOfElement = new List<MeshTriangle>();
            HashSet<MeshTriangle> trianglesParallelToZ = new HashSet<MeshTriangle>();
            HashSet<MeshTriangle> trianglesPerpendicularToZ = new HashSet<MeshTriangle>();

            if (meshes.Count > 0)
            {
                foreach (var mesh in meshes)
                {
                    //AddTrianglesPerpendicularToZ(trianglesPerpendicularToZ, mesh);
                    //AddMeshTriangles(trianglesParallelToZ, mesh);
                    for (int i = 0; i < mesh.NumTriangles; i++)
                    {
                        trianglesOfElement.Add(mesh.get_Triangle(i));
                    }
                }
            }

            foreach (var tri in trianglesOfElement)
            {
                XYZ normal = GetNormalFromTriangle(tri);
                if (GeometryUtility.IsFacePerpendicularToZ(normal))
                {
                    trianglesPerpendicularToZ.Add(tri);
                }
                else if (IsFaceParallelToZ(normal))
                {
                    trianglesParallelToZ.Add(tri);
                }
            }

            //trianglesOfElement.Where(t => !trianglesParallelToZ.Contains(t)).ToList();

            //MeshTriangle tri = trianglesPerpendicularToZ[10];
            //string str = "";
            //List<XYZ> points = GetVerticesOfTriangle(tri);
            //foreach (var p in points)
            //{
            //    str += p.ToString();
            //    str += ", ";
            //}
            //TaskDialog.Show("dfd", str);

            // Xóa những face thuộc trụ và những face thuộc hình tròn ở 2 đầu trụ
            trianglesOfElement.RemoveAll(t => trianglesParallelToZ.Contains(t));
            trianglesOfElement.RemoveAll(t => trianglesPerpendicularToZ.Contains(t));
            //TestFunction(trianglesOfElement, doc);
            List<MeshTriangle> trianglesHighest = GetTrianglesHaveHigherZ(trianglesOfElement, radius, firstPoint, doc);

            //TestFunction(trianglesHighest, doc);

            // TopZ này chỉ phù hợp trong trường hợp tất cả trụ có vị trí bằng nhau
            //double topZ = double.MinValue;
            //foreach (var triangle in trianglesOfElement)
            //{
            //    double maxZOfTriangle = GetHighestAndLowestZPoint(triangle).Max.Z;
            //    if (maxZOfTriangle > topZ)
            //    {
            //        topZ = maxZOfTriangle;
            //    }
            //}
            //List<MeshTriangle> trianglesPresentForTopViewShape = new List<MeshTriangle>();
            //List<MeshTriangle> trianglesPresentForTopViewShape = trianglesOfElement;
            //foreach (var triangle in trianglesOfElement)
            //{
            //    double minZOfTriangle = GetHighestAndLowestZPoint(triangle).Min.Z;
            //    //if (topZ - minZOfTriangle < radius * 2)
            //    //Lấy tất cả các tam giác có minZ phải lớn hơn topPoint.Z của trụ
            //    if (minZOfTriangle > firstPoint.Z)
            //    {
            //        trianglesPresentForTopViewShape.Add(triangle);
            //    }
            //}
            List<XYZ> firstAndLastPoint = new List<XYZ> { firstPoint, lastPoint };

            //FindPointAndAxisOfIt(firstPoint, lastPoint, trianglesPresentForTopViewShape, radius, topZ);

            var tupleValues = GetLinesFromTriangles(trianglesHighest, firstAndLastPoint, radius, true, doc, null);
            if (tupleValues.FirstLine != null && tupleValues.LastLine != null)
            {
                Line firstLine = tupleValues.FirstLine;
                Line lastLine = tupleValues.LastLine;
                // Danh sách các line theo thứ tự nối nhau
                List<Line> lines = tupleValues.Lines;

                //lines.Add(firstLine);
                //lines.Add(lastLine);
                //List<(XYZ, XYZ)> pairs = new List<(XYZ, XYZ)>();
                //foreach (Line line in lines)
                //{
                //    pairs.Add((line.GetEndPoint(0), line.GetEndPoint(1)));
                //}
                lines.RemoveRange(0, 2);

                GetAllIntersecPoints(firstPoint, lastPoint, firstLine, lastLine, lines, doc);
            }

            //var firstPointAndAxis = GetPointAndAxisOfTriangles(firstPoint, trianglesPresentForTopViewShape, radius, topZ, true);
            //var secondPointAndAxis = GetPointAndAxisOfTriangles(lastPoint, trianglesPresentForTopViewShape, radius, topZ, true);

            //Dictionary<XYZ, Line> topPointAndLine = new Dictionary<XYZ, Line>();
            //if (keyValuePairs.Count == 2)
            //{
            //    foreach (var keyPairs in keyValuePairs)
            //    {
            //        Line line = CreateLine(keyPairs.Key, keyPairs.Value);
            //        topPointAndLine.Add(keyPairs.Key, line);
            //    }
            //}
            //List<(XYZ,Line)> pointAndLines = new List<(XYZ,Line)> ();
            //Line firstLine = CreateLine(firstPointAndAxis.OriginPoint, firstPointAndAxis.Axis);
            //Line secondLine = CreateLine(secondPointAndAxis.OriginPoint, secondPointAndAxis.Axis);
            //pointAndLines.Add((firstPointAndAxis.OriginPoint, firstLine));
            //pointAndLines.Add((secondPointAndAxis.OriginPoint, secondLine));

            //List<MeshTriangle> trianglesViewed = new List<MeshTriangle>();
            //foreach (var pointAndLine in topPointAndLine)
            //{
            //    foreach (var triangle in trianglesPresentForTopViewShape)
            //    {
            //        XYZ point1 = triangle.get_Vertex(0);
            //        // Kiểm tra những point trong tam giác nào thỏa mãn nằm các đường line 1 khoảng thì gom vào 1 nhóm
            //        if (DistancePointToLine(point1, pointAndLine.Value) < (topZ - pointAndLine.Key.Z) + tolerance)
            //        {
            //            trianglesViewed.Add(triangle);
            //        }
            //    }
            //}
            //List<MeshTriangle> remainTriangles = GetRemainTriangles(pointAndLines, trianglesPresentForTopViewShape, topZ);

            //List<MeshTriangle> remainTriangles = GetElementsInANotInB(trianglesPresentForTopViewShape, trianglesViewed);
            // Nếu còn các triangles thừa, tức là hình dáng của shape có ít nhất 3 đường
            //if (remainTriangles.Count > 0)
            //{
            //    // Tạo 1 hàm đệ quy lấy ra tia của các tam giác còn lại, có thể nhiều hơn 1 tia
            //}
            //else
            //{
            //Line line1 = topPointAndLine[firstPoint];
            //Line line2 = topPointAndLine[lastPoint];

            // trường hợp vuông góc, tức là 2 đường vuông góc với nhau;
            //if (AreLinesPerpendicular(firstLine, secondLine))
            //{
            //    XYZ intersectPoint = GetIntersectionPoint(firstLine, secondLine);
            //    //Plane plane = CreatePlaneFromLine(line1);
            //    //XYZ pointOnLine2 = line2.GetEndPoint(0);
            //    //XYZ intersectPoint = GetProjectedPoint(plane, pointOnLine2);
            //}
            // trường hợp trùng nhau tức là chỉ có 1 đường thẳng
            // không cần sử lý vì nếu tạo thành 1 đường thẳng thì đó là trường hợp có 2 trụ, đã được sử lý từ trước
            //else if (AreLinesColinear(firstLine, secondLine))
            //{
            //}
            //}
            //}
        }

        /// <summary>
        /// Loại bỏ những triangles cùng vị trí (x,y) nhưng thấp hơn
        /// </summary>
        /// <param name="triangles"></param>
        private List<MeshTriangle> GetTrianglesHaveHigherZ(List<MeshTriangle> triangles, double r, XYZ point, Document doc)
        {
            // Đoạn này dùng để gom nhóm các tam giác thuộc 1 grid
            var squares = GeometryUtility.GenerateGridSquares(minX, maxX, minY, maxY, r * 2);
            //Dictionary<int, List<MeshTriangle>> keyValuePairs = new Dictionary<int, List<MeshTriangle>>();
            List<List<MeshTriangle>> groupTriangles = new List<List<MeshTriangle>>();
            //int key = 0;
            foreach (var square in squares)
            {
                List<MeshTriangle> triangleList = new List<MeshTriangle>();
                foreach (var tri in triangles)
                {
                    var vertices = GetVerticesOfTriangle(tri);
                    // Nếu grid và tam giác giao nhau
                    if (GeometryUtility.AreTriangleAndSquareIntersecting(vertices, square))
                    {
                        triangleList.Add(tri);
                    }
                }
                if (triangleList.Count > 0)
                {
                    groupTriangles.Add(triangleList);
                    //keyValuePairs.Add(key, triangleList);
                }
            }
            // Lặp qua mỗi square, xóa đi những tam giác trong cùng 1 square mà ở dưới so với tam giác khác
            // Kết quả còn lại là mỗi square sẽ có 1 số lượng tam giác ở trên cùng, các tam giác có thể trùng nhau trong
            // square khác
            HashSet<MeshTriangle> meshTrianglesHigher = new HashSet<MeshTriangle>();
            //foreach (var keyValue in keyValuePairs)
            //{
            //    List<MeshTriangle> trianglesOnGrid = keyValue.Value;
            //    HashSet<MeshTriangle> trianglesLower = new HashSet<MeshTriangle>();
            //    // Lặp qua các tam giác trong square đó
            //    for (int i = 0; i < trianglesOnGrid.Count - 1; i++)
            //    {
            //        if (trianglesLower.Contains(trianglesOnGrid[i])) continue;
            //        List<Line> edges1 = GetTriangleEdges(trianglesOnGrid[i]);
            //        double topZ = GetHighestAndLowestZPoint(trianglesOnGrid[i]).Max.Z;
            //        for (int j = i + 1; j < trianglesOnGrid.Count; j++)
            //        {
            //            if (trianglesLower.Contains(trianglesOnGrid[j])) continue;
            //            if (IsTrianglesIntersect(edges1, trianglesOnGrid[j]))
            //            {
            //                if (CompareTwoTriangles(topZ, trianglesOnGrid[j]))
            //                {
            //                    trianglesLower.Add(trianglesOnGrid[j]);
            //                }
            //                else
            //                {
            //                    trianglesLower.Add(trianglesOnGrid[i]);
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //    trianglesOnGrid = trianglesOnGrid.Where(x => !trianglesLower.Contains(x)).ToList();
            //    meshTrianglesHigher.UnionWith(trianglesOnGrid);
            //}
            //List<MeshTriangle> testList = groupTriangles.SelectMany(tri => tri).ToList();

            //TestFunction(testList, doc);
            foreach (var trianglesOnGrid in groupTriangles)
            {
                double maxZ = double.MinValue;
                foreach (var tri in trianglesOnGrid)
                {
                    double highest = GetHighestAndLowestZPoint(tri).Max.Z;

                    if (highest > maxZ)
                    {
                        maxZ = highest;
                    }
                }
                foreach (var tri1 in trianglesOnGrid)
                {
                    if (meshTrianglesHigher.Contains(tri1)) continue;
                    double highest = GetHighestAndLowestZPoint(tri1).Max.Z;
                    if (maxZ - highest < r)
                    {
                        meshTrianglesHigher.Add(tri1);
                    }
                }
            }
            //TestFunction(meshTrianglesHigher.ToList(), doc);
            //triangles.RemoveAll(x => meshTrianglesHigher.Contains(x));
            //TaskDialog.Show("dfd", meshTrianglesHigher.Count.ToString() + ", " + triangles.Count.ToString());

            //double minZ = double.MaxValue;
            //double maxZ = double.MinValue;
            //MeshTriangle minTri = null;

            //foreach (var tri in meshTrianglesHigher)
            //{
            //    double lowest = GetHighestAndLowestZPoint(tri).Min.Z;
            //    double highest = GetHighestAndLowestZPoint(tri).Max.Z;
            //    if (lowest < minZ)
            //    {
            //        minZ = lowest;
            //        minTri = tri;
            //    }
            //    if (highest > maxZ)
            //    {
            //        maxZ = highest;
            //    }
            //}

            //List<MeshTriangle> listThap = new List<MeshTriangle>();
            //List<MeshTriangle> listCao = new List<MeshTriangle>();
            //foreach (var tri in meshTrianglesHigher)
            //{
            //    //double highest = GetHighestAndLowestZPoint(tri).Max.Z;
            //    double lowest = GetHighestAndLowestZPoint(tri).Min.Z;
            //    if (lowest < minZ + r * 3)
            //    {
            //        listThap.Add(tri);
            //    }
            //    if (lowest >= point.Z)
            //    {
            //        listCao.Add(tri);
            //    }
            //}
            //TaskDialog.Show("dfd", "Toan bo danh sach:" + meshTrianglesHigher.Count.ToString() + ", listThap:" + listThap.Count.ToString() + ", listCao: " + listCao.Count.ToString());

            //string str = "";
            //List<XYZ> points = GetVerticesOfTriangle(minTri);
            //foreach (var p in points)
            //{
            //    str += p.ToString();
            //    str += ", ";
            //}
            //TaskDialog.Show("dfd", str);
            //XYZ newPoint = SetOriginPoint(point, minZ);
            //XYZ newPoint1 = SetOriginPoint(point, maxZ);
            //TaskDialog.Show("dfd", newPoint.ToString() + newPoint1.ToString());

            //foreach (var keyValue in keyValuePairs)
            //{
            //    meshTrianglesHigher.UnionWith(keyValue.Value);
            //}
            return meshTrianglesHigher.ToList();

            //List<MeshTriangle> trianglesLowest = new List<MeshTriangle>();
            //List<MeshTriangle> trianglesViewed = new List<MeshTriangle>();
            //for (int i = 0; i < triangles.Count - 1; i++)
            //{
            //    List<XYZ> listVertex1 = GetVerticesOfTriangle(triangles[i]);
            //    for (int j = i + 1; j < triangles.Count; j++)
            //    {
            //        List<XYZ> listVertex2 = GetVerticesOfTriangle(triangles[j]);
            //        if (HasCommonVertex(listVertex1, listVertex2))
            //        {
            //            break;
            //        }
            //        if (IsTrianglesIntersect(triangles[i], triangles[j]))
            //        {
            //            MeshTriangle meshTriangle = GetTriangleLowest(triangles[i], triangles[j]);
            //            trianglesLowest.Add(meshTriangle);
            //        }
            //    }
            //}
            //triangles.RemoveAll(tr => trianglesLowest.Contains(tr));
        }

        /// <summary>
        /// Lấy ra triangle có điểm thấp nhất
        /// </summary>
        /// <param name="tri1"></param>
        /// <param name="tri2"></param>
        /// <returns></returns>
        private bool CompareTwoTriangles(double tri1TopZ, MeshTriangle tri2)
        {
            if (tri1TopZ >= GetHighestAndLowestZPoint(tri2).Max.Z)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Hàm kiểm tra xem 2 tam giác khi cho đồng phẳng có giao nhau hay không
        /// </summary>
        /// <param name="tri1"></param>
        /// <param name="tri2"></param>
        /// <returns></returns>
        private bool IsTrianglesIntersect(List<Line> edges1, MeshTriangle tri2)
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

                    //if (l1.Intersect(l2, out IntersectionResultArray result) == SetComparisonResult.)
                    //{
                    //    return true;
                    //}
                }
            }

            return false;
        }

        /// <summary>
        /// Hàm tạo line nếu khoảng cách các điểm là phù hợp
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        private void CreateLineFromTriangleEdges(XYZ p1, XYZ p2, List<Line> lines)
        {
            try
            {
                Line line = Line.CreateBound(p1, p2);
                lines.Add(line);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Hàm tạo line từ các cạnh của triangle
        /// </summary>
        /// <param name="tri"></param>
        /// <returns></returns>
        private List<Line> GetTriangleEdges(MeshTriangle tri)
        {
            var a = SetOriginPoint(tri.get_Vertex(0), 0);
            var b = SetOriginPoint(tri.get_Vertex(1), 0);
            var c = SetOriginPoint(tri.get_Vertex(2), 0);

            List<Line> lines = new List<Line>();

            CreateLineFromTriangleEdges(a, b, lines);
            CreateLineFromTriangleEdges(b, c, lines);
            CreateLineFromTriangleEdges(c, a, lines);
            return lines;
            //return new List<Line>
            //{
            //    Line.CreateBound(a, b),
            //    Line.CreateBound(b, c),
            //    Line.CreateBound(c, a)
            //};
        }

        /// <summary>
        /// Hàm tạo plane song song với trục Z từ 1 line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private Plane CreatePlaneParallelToZFromLine(Line line)
        {
            XYZ dir = line.Direction.Normalize();
            double dot = dir.DotProduct(XYZ.BasisZ);
            if (Math.Abs(Math.Abs(dot) - 1) > tolerance)
            {
                XYZ p1 = line.GetEndPoint(0);
                XYZ p2 = line.GetEndPoint(1);
                XYZ p3 = SetOriginPoint(p1, 0);
                Plane plane = Plane.CreateByThreePoints(p1, p2, p3);
                return plane;
            }
            return null;
        }

        /// <summary>
        /// Hàm kiểm tra 2 vector có vuông góc không
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private bool AreVectorsPerpendicular(XYZ v1, XYZ v2)
        {
            if (v1.IsZeroLength() || v2.IsZeroLength())
                return false;  // Vector rỗng không có hướng xác định

            double dot = v1.Normalize().DotProduct(v2.Normalize());
            return Math.Abs(dot) < 1e-4;
        }

        /// <summary>
        /// Hàm chiếu 1 line lên 1 plane và tạo ra line mới
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private Line CreateLineOnPlane(Plane plane, Line line)
        {
            XYZ p1 = line.GetEndPoint(0);
            XYZ p2 = line.GetEndPoint(1);
            XYZ newPoint1 = GetProjectedPoint(plane, p1);
            XYZ newPoint2 = GetProjectedPoint(plane, p2);

            Line newLine = Line.CreateBound(newPoint1, newPoint2);
            return newLine;
        }

        /// <summary>
        /// Hàm lấy ra danh sách các cặp giao điểm của các line
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="lastPoint"></param>
        /// <param name="firstLine"></param>
        /// <param name="lastLine"></param>
        /// <param name="lines"></param>
        /// <returns></returns>
        private List<(XYZ, XYZ)> GetAllIntersecPoints(XYZ firstPoint, XYZ lastPoint, Line firstLine, Line lastLine, List<Line> lines, Document doc)
        {
            List<(XYZ, XYZ)> pairs = new List<(XYZ, XYZ)>();
            List<XYZ> points = new List<XYZ>();
            points.Add(firstPoint);
            if (lines.Count > 0)
            {
                lines.Insert(0, firstLine);
                lines.Add(lastLine);

                for (int i = 0; i < lines.Count - 1; i++)
                {
                    XYZ dir1 = lines[i].Direction.Normalize();
                    XYZ dir2 = lines[i + 1].Direction.Normalize();
                    // Tạo mặt phẳng đi qua 1 line và thẳng đứng song song với trục Z
                    Plane plane = CreatePlaneParallelToZFromLine(lines[i]);
                    // Nếu line[i + 1] song song với plane
                    if (AreVectorsPerpendicular(plane.Normal.Normalize(), dir2))
                    {
                        lines[i + 1] = CreateLineOnPlane(plane, lines[i + 1]);
                        // Sau đó sẽ xét giao điểm
                    }
                    // Nếu line[i + 1] vuông góc với plane
                    else if (IsParallel(plane.Normal.Normalize(), dir2))
                    {
                        // tạo mặt phẳng mới đi qua line1 và song song với line 2
                        XYZ cross = dir1.CrossProduct(dir2);
                        if (!cross.IsZeroLength())
                        {
                            Plane newPlane = Plane.CreateByNormalAndOrigin(cross, lines[i].GetEndPoint(0));
                            lines[i + 1] = CreateLineOnPlane(newPlane, lines[i + 1]);
                        }
                    }
                }

                //lines.Add(firstLine);
                //lines.Add(lastLine);

                // Tìm giao điểm giữa các line với nhau, nếu có 2 giao điểm thì là các line ở giữa
                // còn có 1 giao điểm thì là 1 trong 2 line đầu hoặc cuối

                for (int i = 0; i < lines.Count - 1; i++)
                {
                    XYZ intersectPoint = GetIntersectionPoint(lines[i], lines[i + 1]);
                    if (intersectPoint != null)
                    {
                        points.Add(intersectPoint);
                    }
                }
                Plane lastPlane = CreatePlaneParallelToZFromLine(lines.Last());
                points.Add(GetProjectedPoint(lastPlane, lastPoint));
                //points.Add(lastPoint);
                for (int i = 0; i < points.Count - 1; i++)
                {
                    pairs.Add((points[i], points[i + 1]));
                }
                //for (int i = 0; i < lines.Count - 1; i++)
                //{
                //    List<XYZ> intersectPoints = new List<XYZ>();
                //    for (int j = i + 1; j < lines.Count; j++)
                //    {
                //        XYZ intersectPoint = GetIntersectionPoint(lines[i], lines[j]);
                //        if (intersectPoint != null)
                //        {
                //            intersectPoints.Add(intersectPoint);
                //        }
                //    }
                //    if (intersectPoints.Count == 2)
                //    {
                //        pairs.Add((intersectPoints[0], intersectPoints[1]));
                //    }
                //    else if (intersectPoints.Count == 1)
                //    {
                //        if (IsPointOnLine(firstPoint, lines[i]))
                //        {
                //            pairs.Add((firstPoint, intersectPoints.First()));
                //        }
                //        else if (IsPointOnLine(lastPoint, lines[i]))
                //        {
                //            pairs.Add((lastPoint, intersectPoints.First()));
                //        }
                //    }
                //}
            }
            // Chỉ có 2 đường
            else
            {
                //if (AreLinesPerpendicular(firstLine, lastLine))
                //{
                // Nếu 2 đường không trùng nhau
                if (!AreLinesColinear(firstLine, lastLine))
                {
                    XYZ intersectPoint = GetIntersectionPoint(firstLine, lastLine);
                    if (intersectPoint != null)
                    {
                        pairs.Add((firstPoint, intersectPoint));
                        pairs.Add((intersectPoint, lastPoint));
                    }
                }
                //}
            }
            //string str = "";
            //foreach (var a in pairs)
            //{
            //    str += a.ToString();
            //    str += ";";
            //}
            //TaskDialog.Show("adfd", str);
            foreach (var pair in pairs)
            {
                CreateModelLine(doc, pair.Item1, pair.Item2);
            }

            return pairs;
        }

        /// <summary>
        /// Hàm kiểm tra xem 1 điểm có thuộc 1 đường thẳng hay không
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        private bool IsPointOnLine(XYZ point, Line line)
        {
            XYZ start = line.GetEndPoint(0);
            XYZ end = line.GetEndPoint(1);

            // Vector từ start đến end và từ start đến point
            XYZ lineVec = end - start;
            XYZ pointVec = point - start;

            // Nếu độ dài của lineVec là 0 (line sai), trả về false
            if (lineVec.IsZeroLength())
                return point.IsAlmostEqualTo(start, tolerance);

            // Kiểm tra xem hai vector có cùng hướng (tức là tích có hướng gần bằng 0)
            XYZ cross = lineVec.CrossProduct(pointVec);
            if (cross.GetLength() > tolerance)
                return false;

            if ((point.DistanceTo(end) + point.DistanceTo(start)) - start.DistanceTo(end) > tolerance)
                return false;

            return true;
        }

        private (Line FirstLine, Line LastLine, List<Line> Lines) GetLinesFromTriangles(List<MeshTriangle> triangles, List<XYZ> points, double radius, bool isTopPointOfCylinder, Document doc, XYZ endTargetPoint)
        {
            List<Line> lines = new List<Line>();
            Line firstLine = null;
            Line lastLine = null;
            List<(XYZ, Line)> pointAndLines = new List<(XYZ, Line)>();
            foreach (var p in points)
            {
                var pointAndAxis = GetPointAndAxisOfTriangles(p, triangles, radius, isTopPointOfCylinder);
                Line line = CreateLine(pointAndAxis.OriginPoint, pointAndAxis.Axis);
                pointAndLines.Add((pointAndAxis.OriginPoint, line));
            }

            if (points.Count == 2)
            {
                firstLine = pointAndLines[0].Item2;
                lastLine = pointAndLines[1].Item2;

                XYZ p1 = firstLine.GetEndPoint(0);
                XYZ p2 = firstLine.GetEndPoint(1);

                XYZ pt1 = lastLine.GetEndPoint(0);
                XYZ pt2 = lastLine.GetEndPoint(1);
            }
            else if (points.Count == 1)
            {
                lines.Add(pointAndLines.FirstOrDefault().Item2);
            }

            List<XYZ> targetPoints = GetRemainTriangles(pointAndLines, triangles, radius, doc);
            //if (triangles.Count > 0)
            //{
            //    if (targetPoints.Count > 0)
            //    {
            //        List<XYZ> newPoints = new List<XYZ> { targetPoints.First() };
            //        lines.AddRange(GetLinesFromTriangles(triangles, newPoints, radius, false, doc).Lines);
            //    }
            //}

            if (targetPoints.Count == 2 && endTargetPoint == null)
            {
                endTargetPoint = targetPoints[1];
            }

            if (targetPoints.Count > 0)
            {
                if (endTargetPoint.DistanceTo(targetPoints.First()) < radius * 6)
                {
                    return (firstLine, lastLine, lines);
                }
                List<XYZ> newPoints = new List<XYZ> { targetPoints.First() };
                lines.AddRange(GetLinesFromTriangles(triangles, newPoints, radius, false, doc, endTargetPoint).Lines);
            }
            return (firstLine, lastLine, lines);
        }

        /// <summary>
        /// Hàm lấy ra tập hợp các tam giác cùng phương với 1 line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="triangles"></param>
        /// <returns></returns>
        private List<MeshTriangle> GetTrianglesParallelToLine(Line line, List<MeshTriangle> triangles)
        {
            List<MeshTriangle> result = new List<MeshTriangle>();
            XYZ normalizedDirection = line.Direction.Normalize();
            foreach (var triangle in triangles)
            {
                XYZ normal = GetNormalFromTriangle(triangle);

                // Nếu normal vuông góc với direction thì dot product gần 0
                double dot = normal.Normalize().DotProduct(normalizedDirection);

                if (Math.Abs(dot) < tolerance)
                {
                    result.Add(triangle);
                }
            }
            return result;
        }

        /// <summary>
        /// Hàm xóa đi những tam giác thuộc line và trả về danh sách các điểm làm mốc để xét đối với những line khác
        /// </summary>
        /// <param name="pointAndLines"></param>
        /// <param name="triangles"></param>
        /// <param name="topZ"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private List<XYZ> GetRemainTriangles(List<(XYZ, Line)> pointAndLines, List<MeshTriangle> triangles, double r, Document doc)
        {
            List<XYZ> targetPoints = new List<XYZ>();
            List<MeshTriangle> trianglesViewed = new List<MeshTriangle>();
            bool isOrigin = false;
            if (pointAndLines.Count == 2)
            {
                isOrigin = true;
            }
            foreach (var tuple in pointAndLines)
            {
                Line line = tuple.Item2;
                XYZ p1 = line.GetEndPoint(0);
                XYZ p2 = line.GetEndPoint(1);

                //CreateModelLine(doc, p1, p2);
                //double distanceFromOriginToLine = DistancePointToLine(tuple.Item1, tuple.Item2);

                // trường hợp line song song với mặt phẳng XY

                //if (Math.Abs(tuple.Item2.Direction.DotProduct(XYZ.BasisZ)) < tolerance && isOrigin)
                //{
                //    distance = (topZ - tuple.Item1.Z) + 0.04;
                //}
                //else
                //{
                //    distance = r * 6;
                //}

                double distance = r * 6;
                List<MeshTriangle> trianglesNearLine = new List<MeshTriangle>();
                foreach (var triangle in triangles)
                {
                    XYZ point1 = triangle.get_Vertex(0);

                    // Kiểm tra những point trong tam giác nào thỏa mãn nằm cách đường line 1 khoảng thì gom vào 1 nhóm
                    //if (DistancePointToLine(point1, tuple.Item2) < (topZ - tuple.Item1.Z) + 0.04)
                    if (DistancePointToLine(point1, tuple.Item2) < distance)
                    {
                        trianglesNearLine.Add(triangle);
                    }
                }
                // Danh sách tam giác song song với line
                List<MeshTriangle> trianglesParallelToLine = GetTrianglesParallelToLine(tuple.Item2, trianglesNearLine);
                XYZ farthestPoint = GetFarthestPoint(tuple.Item1, trianglesParallelToLine);
                targetPoints.Add(farthestPoint);
                trianglesViewed.AddRange(trianglesParallelToLine);
            }
            //List<MeshTriangle> remainTriangles = GetElementsInANotInB(triangles, trianglesViewed);
            triangles.RemoveAll(x => trianglesViewed.Contains(x));
            return targetPoints;
        }

        /// <summary>
        /// Trong danh sách các tam giác, lấy ra tất cả các điểm, sau đó chọn ra điểm có khoảng cách xa nhất
        /// với điểm cho trước
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <returns></returns>
        private XYZ GetFarthestPoint(XYZ targetPoint, List<MeshTriangle> triangles)
        {
            List<XYZ> vertexs = GetVerticesOfAllTriangles(triangles);
            double maxDistance = double.MinValue;
            XYZ farthestPoint = null;
            foreach (var p in vertexs)
            {
                if (targetPoint.DistanceTo(p) > maxDistance)
                {
                    maxDistance = targetPoint.DistanceTo(p);
                    farthestPoint = p;
                }
            }
            return farthestPoint;
        }

        //private void GetLineFromRemainTriangles(List<MeshTriangle> triangles, double topZ)
        //{
        //    List<MeshTriangle> trianglesViewed = new List<MeshTriangle>();
        //    foreach (var pointAndLine in topPointAndLine)
        //    {
        //        foreach (var triangle in triangles)
        //        {
        //            XYZ point1 = triangle.get_Vertex(0);
        //            // Kiểm tra những point trong tam giác nào thỏa mãn nằm các đường line 1 khoảng thì gom vào 1 nhóm
        //            if (DistancePointToLine(point1, pointAndLine.Value) < (topZ - pointAndLine.Key.Z) + tolerance)
        //            {
        //                trianglesViewed.Add(triangle);
        //            }
        //        }
        //    }
        //    List<MeshTriangle> remainTriangles = GetElementsInANotInB(triangles, trianglesViewed);
        //    if (remainTriangles.Count > 0)
        //    {
        //        GetLineFromRemainTriangles(remainTriangles, topZ);
        //    }
        //}

        private XYZ GetIntersectionPoint(Line line1, Line line2)
        {
            IntersectionResultArray resultArray;
            SetComparisonResult result = line1.Intersect(line2, out resultArray);

            if (result == SetComparisonResult.Overlap && resultArray != null && resultArray.Size > 0)
            {
                // Lấy điểm đầu tiên (thường chỉ có 1 điểm với đường thẳng)
                return resultArray.get_Item(0).XYZPoint;
            }

            // Không có giao điểm
            return null;
        }

        //private Plane CreatePlaneFromLine(Line line)
        //{
        //    XYZ origin = line.GetEndPoint(0);
        //    Plane plane = Plane.CreateByNormalAndOrigin(origin, XYZ.BasisZ);
        //    return plane;
        //}

        /// <summary>
        /// Kiểm tra xem 2 đường có trùng nhau không
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        private bool AreLinesColinear(Line line1, Line line2)
        {
            // Vector hướng
            XYZ dir1 = (line1.GetEndPoint(1) - line1.GetEndPoint(0)).Normalize();
            XYZ dir2 = (line2.GetEndPoint(1) - line2.GetEndPoint(0)).Normalize();

            // Kiểm tra song song (cross product gần 0 vector)
            XYZ cross = dir1.CrossProduct(dir2);
            bool areParallel = cross.GetLength() < tolerance;

            if (!areParallel)
                return false;

            // Kiểm tra cùng phương (vector nối 2 gốc nằm trên cùng đường thẳng)
            XYZ vectorBetween = line2.GetEndPoint(0) - line1.GetEndPoint(0);
            XYZ cross2 = vectorBetween.CrossProduct(dir1);
            return cross2.GetLength() < tolerance;
        }

        private bool AreLinesPerpendicular(Line line1, Line line2)
        {
            XYZ dir1 = (line1.GetEndPoint(1) - line1.GetEndPoint(0)).Normalize();
            XYZ dir2 = (line2.GetEndPoint(1) - line2.GetEndPoint(0)).Normalize();

            double dot = dir1.DotProduct(dir2);
            return Math.Abs(dot) < cosineAngleTolerance;
        }

        private Line CreateLine(XYZ origin, XYZ direction)
        {
            // Nên giới han chiều dài của Line tránh trường hợp 2 line vuông góc với nhau, và do quá dài nên cắt nhau, gây ra sai điểm giao
            double length = 5;
            //XYZ midPoint = SetOriginPoint(origin, (origin.Z + topZ) / 2);
            XYZ p1 = origin + direction.Multiply(-length);
            XYZ p2 = origin + direction.Multiply(length);

            return Line.CreateBound(p1, p2);
        }

        private List<MeshTriangle> GetElementsInANotInB(List<MeshTriangle> A, List<MeshTriangle> B)
        {
            var result = new List<MeshTriangle>();

            foreach (var item in A)
            {
                if (!B.Contains(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// Hàm tính khoảng cách từ 1 point đến 1 line
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private double DistancePointToLine(XYZ point, Line line)
        {
            XYZ lineOrigin = line.GetEndPoint(0);
            XYZ lineDirection = (line.GetEndPoint(1) - lineOrigin).Normalize();
            XYZ vectorToPoint = point - lineOrigin;

            XYZ cross = vectorToPoint.CrossProduct(lineDirection);
            double distance = cross.GetLength();

            return distance;
        }

        private bool IsPerpendicularToXAxis(XYZ v)
        {
            return Math.Abs(v.X) < tolerance;
        }

        private bool IsPerpendicularToYAxis(XYZ v)
        {
            return Math.Abs(v.Y) < tolerance;
        }

        private XYZ GetOrthogonalVectorForm(XYZ B)
        {
            // Check if y_b is zero to avoid division by zero
            if (B.Y == 0)
                return null; // Không thể tìm vector dạng (0, ya, 1) vuông góc với B

            double ya = -B.X / B.Y;
            return new XYZ(1, ya, 0);
        }

        /// <summary>
        /// Lấy ra danh sách các tam giác cách 1 điêm cho trước 1 khoảng và thỏa mãn có 1 cạnh
        /// có chiều dài lớn hơn 3 r
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="point"></param>
        /// <param name="length"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private List<MeshTriangle> GetSublistTriangles(List<MeshTriangle> triangles, XYZ point, double length, double radius)
        {
            List<MeshTriangle> sublist = new List<MeshTriangle>();
            foreach (var tr in triangles)
            {
                List<XYZ> vertices = GetVerticesOfTriangle(tr);
                foreach (var pt in vertices)
                {
                    double dist = pt.DistanceTo(point);
                    if (dist < length + radius * 2)
                    {
                        int count = 0;
                        List<Line> edges = GetTriangleEdges(tr);
                        foreach (Line line in edges)
                        {
                            if (line.Length > radius * 3)
                            {
                                count++;
                                break;
                            }
                        }
                        if (count > 0)
                        {
                            sublist.Add(tr);
                            break;
                        }
                    }
                }

                //int count = 0;
                //List<Line> edges = GetTriangleEdges(tr);
                //foreach (Line line in edges)
                //{
                //    if (line.Length > radius * 3)
                //    {
                //        count++;
                //        break;
                //    }
                //}
                //if (count > 0)
                //{
                //    sublist.Add(tr);
                //    break;
                //List<XYZ> vertices = GetVerticesOfTriangle(tr);
                //foreach (var pt in vertices)
                //{
                //    double dist = pt.DistanceTo(point);
                //    if (dist < length + radius * 2)
                //    {
                //        sublist.Add(tr);
                //        break;
                //    }
                //}
                //}
                //else
                //{
                //    break;
                //}
            }
            return sublist;
        }

        /// <summary>
        /// Hàm lấy ra danh sách các điểm của tất cả các triangles
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        private List<XYZ> GetVerticesOfAllTriangles(List<MeshTriangle> triangles)
        {
            List<XYZ> vertexs = new List<XYZ>();
            foreach (var tri in triangles)
            {
                vertexs.AddRange(GetVerticesOfTriangle(tri));
            }
            return vertexs;
        }

        /// <summary>
        /// Hàm trả về 1 cặp điểm và vector chỉ phương để vẽ line
        /// </summary>
        /// <param name="A"></param>
        /// <param name="triangles"></param>
        /// <param name="r"></param>
        /// <param name="topZ"></param>
        /// <param name="IsOrigin"></param>
        /// <returns></returns>
        private (XYZ OriginPoint, XYZ Axis) GetPointAndAxisOfTriangles(XYZ A, List<MeshTriangle> triangles, double r, bool IsOrigin)
        {
            //XYZ pointA = SetOriginPoint(A, topZ);
            List<XYZ> vertexs = GetVerticesOfAllTriangles(triangles);
            //foreach (var tri in triangles)
            //{
            //    vertexs.AddRange(GetVerticesOfTriangle(tri));
            //    //XYZ v1 = tri.get_Vertex(0);
            //    //XYZ v2 = tri.get_Vertex(1);
            //    //XYZ v3 = tri.get_Vertex(2);
            //    //vertexs.Add(v1);
            //    //vertexs.Add(v2);
            //    //vertexs.Add(v3);
            //}
            // Điểm gần A nhất có chiều cao = topZ
            //XYZ B = null;

            //double minDisToA = double.MaxValue;

            //foreach (var v in vertexs)
            //{
            //    if (Math.Abs(v.Z - topZ) < r / 2)
            //    {
            //        double distToA = A.DistanceTo(v);
            //        if (distToA < minDisToA)
            //        {
            //            minDisToA = distToA;
            //            B = v;
            //        }
            //    }
            //}

            XYZ B = null;

            double maxDisToA = double.MinValue;
            //double minZ = double.MaxValue;
            //double maxZ = double.MinValue;
            //XYZ minPoint;
            //XYZ maxPoint;
            //foreach (var v in vertexs)
            //{
            //    if (v.Z < minZ)
            //    {
            //        minZ = v.Z;
            //        minPoint = v;
            //    }
            //    if (v.Z > maxZ)
            //    {
            //        maxZ = v.Z;
            //        maxPoint = v;
            //    }
            //}

            bool x = IsOrigin;
            foreach (var v in vertexs)
            {
                //if (Math.Abs(v.Z - A.Z) < r * 6)
                if (A.DistanceTo(v) < r * 6)
                {
                    double distToA = A.DistanceTo(v);
                    if (distToA > maxDisToA)
                    {
                        maxDisToA = distToA;
                        B = v;
                    }
                }
            }

            // Khoảng cách từ A đến B cộng thêm 1 khoảng 2r

            double length = A.DistanceTo(B);

            List<MeshTriangle> subListTriangleNearB = GetSublistTriangles(triangles, A, length, r);

            // Lấy ra danh sách các triangles các điểm A 1 khoảng từ A đến closestToA
            //List<MeshTriangle> subListTriangleNearA = GetSublistTriangles(triangles, A, length - tolerance);

            //var trianglesInBNotInA = subListTriangleNearB.Where(a => !subListTriangleNearA.Contains(a)).ToList();
            XYZ axis = GetAxisFromTriangles(subListTriangleNearB);
            if (IsOrigin)
            {
                return (A, axis);
            }
            else
            {
                //XYZ newB = SetOriginPoint(B, A.Z);
                //return (newB, axis);
                return (B, axis);
            }
        }

        //private Dictionary<XYZ, XYZ> FindPointAndAxisOfIt(XYZ A, XYZ B, List<MeshTriangle> triangles, double r, double topZ)
        //{
        //    XYZ pointA = SetOriginPoint(A, topZ);
        //    XYZ pointB = SetOriginPoint(B, topZ);

        //    Dictionary<XYZ, XYZ> keyValuePairs = new Dictionary<XYZ, XYZ>();

        //    List<XYZ> vertexs = new List<XYZ>();
        //    foreach (var tri in triangles)
        //    {
        //        XYZ v1 = tri.get_Vertex(0);
        //        XYZ v2 = tri.get_Vertex(1);
        //        XYZ v3 = tri.get_Vertex(2);
        //        vertexs.Add(v1);
        //        vertexs.Add(v2);
        //        vertexs.Add(v3);
        //    }
        //    XYZ closestToA = null;
        //    XYZ closestToB = null;
        //    double minDisToA = double.MaxValue;
        //    double minDisToB = double.MaxValue;
        //    foreach (var v in vertexs)
        //    {
        //        if (v.Z == topZ)
        //        {
        //            double distToA = pointA.DistanceTo(v);
        //            if (distToA < minDisToA)
        //            {
        //                minDisToA = distToA;
        //                closestToA = v;
        //            }

        //            double distToB = pointB.DistanceTo(v);
        //            if (distToB < minDisToB)
        //            {
        //                minDisToB = distToB;
        //                closestToB = v;
        //            }
        //        }
        //    }

        //    List<MeshTriangle> subListTriangleNearAPoint = new List<MeshTriangle>();
        //    List<MeshTriangle> subListTriangleNearBPoint = new List<MeshTriangle>();
        //    foreach (var tr in triangles)
        //    {
        //        XYZ pt = tr.get_Vertex(0);
        //        double distToA = pt.DistanceTo(closestToA);
        //        if (distToA > r && distToA < r * 6)
        //        {
        //            subListTriangleNearAPoint.Add(tr);
        //        }

        //        double distToB = pt.DistanceTo(pointB);
        //        if (distToB > r && distToB < r * 6)
        //        {
        //            subListTriangleNearBPoint.Add(tr);
        //        }
        //    }

        //    XYZ firstAxis = GetAxisFromTriangles(subListTriangleNearAPoint);
        //    XYZ secondAxis = GetAxisFromTriangles(subListTriangleNearBPoint);
        //    keyValuePairs.Add(A, firstAxis);
        //    keyValuePairs.Add(B, secondAxis);
        //    return keyValuePairs;
        //}

        /// <summary>
        /// Tìm tia song song với trục của trụ bằng cách lấy ra 2 tam giác bất kỳ có normal không
        /// song song với nhau, sau đó cùng crossproduct 2 normal đó
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        private XYZ GetAxisFromTriangles(List<MeshTriangle> triangles)
        {
            MeshTriangle firstTriangle = triangles.FirstOrDefault();
            XYZ firstNormal = GetNormalFromTriangle(firstTriangle);
            XYZ secondNormal = null;
            MeshTriangle secondTriangle = null;
            for (int i = 1; i < triangles.Count; i++)
            {
                XYZ normal = GetNormalFromTriangle(triangles[i]);
                // Nếu 2 vector không song song với nhau
                if (!IsParallel(firstNormal, normal))
                {
                    secondTriangle = triangles[i];
                    secondNormal = normal;
                    break;
                }
            }
            XYZ axisVector = null;
            if (secondNormal != null)
            {
                axisVector = firstNormal.CrossProduct(secondNormal).Normalize();
            }
            if (IsParallel(axisVector, XYZ.BasisZ))
            {
                return SetOriginPoint(axisVector, 0);
            }

            return axisVector;
        }

        /// <summary>
        /// Hàm vẽ Arc từ 3 điểm bất kỳ
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="doc"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void CreateModelArcFrom3Points(UIDocument uiDoc, Document doc, XYZ point1, XYZ point2, XYZ point3)
        {
            ModelCurve modelCurve = null;
            using (Transaction trans = new Transaction(doc, "Create Model Arc From 3 Points"))
            {
                trans.Start();

                XYZ p1 = transform.OfPoint(point1);
                XYZ p2 = transform.OfPoint(point2);
                XYZ p3 = transform.OfPoint(point3);

                XYZ newVector = XYZ.BasisZ.Multiply(1);
                XYZ pt1 = p1.Add(newVector);
                XYZ pt2 = p2.Add(newVector);
                XYZ pt3 = p3.Add(newVector);

                // Tạo cung từ 3 điểm
                Arc arc = Arc.Create(pt1, pt2, pt3);

                // Tính mặt phẳng chứa cung: pháp tuyến = tích có hướng giữa 2 vector bất kỳ trên mặt cong
                XYZ v1 = (pt2 - pt1).Normalize();
                XYZ v2 = (pt3 - pt1).Normalize();
                XYZ normal = v1.CrossProduct(v2).Normalize();

                // Kiểm tra normal hợp lệ
                if (normal.IsZeroLength())
                    throw new System.InvalidOperationException("3 điểm thẳng hàng – không thể tạo cung.");

                Plane plane = Plane.CreateByNormalAndOrigin(normal, pt1);
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                modelCurve = doc.Create.NewModelCurve(arc, sketchPlane);

                trans.Commit();
            }
            uiDoc.ShowElements(modelCurve.Id);
        }

        /// <summary>
        /// Hàm này kiểm tra xem trong danh sách hình trụ của tất cả các element
        /// có tồn tại toppoint của  hình trụ A bằng bottom point của hình trụ B không và ngược lại,
        /// nếu bằng thì merge chúng lại, lấy maxtop Z và minbottom Z
        /// </summary>
        /// <param name="cylinderInfos"></param>
        /// <returns>Trả về 1 list<CylinderInfo> sau khi đã merge</returns>
        private List<CylinderInfo> MergeCylinderInfos(List<CylinderInfo> cylinderInfos)
        {
            var visited = new HashSet<CylinderInfo>();
            var result = new List<CylinderInfo>();

            foreach (var item in cylinderInfos)
            {
                if (!visited.Contains(item))
                {
                    var group = new List<CylinderInfo>();
                    CollectConnected(item, cylinderInfos, group, visited);

                    if (group.Count == 1)
                    {
                        result.Add(group[0]); // không có kết nối
                    }
                    else
                    {
                        double maxTop = group.Max(a => a.TopPoint.Z);
                        double minBottom = group.Min(a => a.BottomPoint.Z);

                        CylinderInfo cylinderInfo = group.FirstOrDefault();
                        cylinderInfo.TopPoint = SetOriginPoint(cylinderInfo.TopPoint, maxTop);
                        cylinderInfo.BottomPoint = SetOriginPoint(cylinderInfo.BottomPoint, minBottom);
                        result.Add(cylinderInfo);
                    }
                }
            }

            return result;
        }

        private void CollectConnected(CylinderInfo current, List<CylinderInfo> input, List<CylinderInfo> group, HashSet<CylinderInfo> visited)
        {
            if (visited.Contains(current)) return;

            visited.Add(current);
            group.Add(current);

            foreach (var other in input)
            {
                if (!visited.Contains(other))
                {
                    if (Math.Abs(current.TopPoint.X - other.TopPoint.X) < tolerance && Math.Abs(current.TopPoint.Y - other.TopPoint.Y) < tolerance)
                    {
                        CollectConnected(other, input, group, visited);
                    }
                }
            }
        }

        /// <summary>
        /// Hàm này dùng để lấy ra danh sách các planarface và cylindricalface từ 1 solid
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="solid"></param>
        /// <returns>Trả về 1 tuple chứa danh sách planarface và cylindricalface </returns>
        private (List<PlanarFace>, List<CylindricalFace>) GetGroupedFacesFromSolid(Document doc, Solid solid)
        {
            List<PlanarFace> planarFaces = new List<PlanarFace>();
            List<CylindricalFace> cylindricalFaces = new List<CylindricalFace>();

            if (solid.Faces.Size > 0 && solid.Volume > 0)
            {
                foreach (Face face in solid.Faces)
                {
                    if (face is PlanarFace planarFace)
                    {
                        planarFaces.Add(planarFace);
                    }
                    else if (face is CylindricalFace cylindricalFace)
                    {
                        cylindricalFaces.Add(cylindricalFace);
                    }
                }
            }
            return (planarFaces, cylindricalFaces);
        }

        /// <summary>
        /// Từ geometry của element, lấy ra danh sách các solid của element đó
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private (List<Solid> Solids, List<Mesh> Meshs) GetSolids(Element element, Document doc)
        {
            Options options = new Options();
            options.IncludeNonVisibleObjects = true;
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;
            GeometryElement elementGeo = element.get_Geometry(options);

            List<Solid> solids = new List<Solid>();
            List<Mesh> meshs = new List<Mesh>();

            foreach (GeometryObject geometryObj in elementGeo)
            {
                if (geometryObj is Solid solid)
                {
                    solids.Add(solid);
                }
                if (geometryObj is GeometryInstance geomInstance)
                {
                    GeometryElement instanceGeometry = geomInstance.GetInstanceGeometry();
                    foreach (GeometryObject geometryObject in instanceGeometry)
                    {
                        if (geometryObject is Solid nestedSolid)
                        {
                            solids.Add(nestedSolid);
                        }
                        if (geometryObject is Mesh mesh)
                        {
                            meshs.Add(mesh);
                        }
                    }
                }
            }
            return (solids, meshs);
        }

        /// <summary>
        /// Lấy ra danh sách các CylinderInfo được tạo ra từ các CylindricalFace
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private (List<Mesh> Meshs, List<CylinderInfo> CylinderInfos) GetCylinderInfosFromElements(Element element, Document doc)
        {
            List<CylinderInfo> cylinderInfos = new List<CylinderInfo>();

            var tupleValue = GetSolids(element, doc);
            List<Solid> solids = tupleValue.Solids;

            List<Mesh> meshes = tupleValue.Meshs;

            HashSet<MeshTriangle> triangles = new HashSet<MeshTriangle>();
            if (meshes.Count > 0)
            {
                foreach (var mesh in meshes)
                {
                    AddMeshTriangles(triangles, mesh);
                }
            }

            if (solids.Count > 0)
            {
                List<PlanarFace> planarFacesOfElement = new List<PlanarFace>();
                foreach (Solid solid in solids)
                {
                    var tuple = GetGroupedFacesFromSolid(doc, solid);
                    List<PlanarFace> planarFacesOfSolid = tuple.Item1;
                    List<CylindricalFace> cylindricalFaces = tuple.Item2;

                    if (planarFacesOfSolid.Count > 0 && cylindricalFaces.Count > 0)
                    {
                        // Lấy ra tất cả các mesh từ các face
                        AddMesh(meshes, planarFacesOfSolid);
                        AddMesh(meshes, cylindricalFaces);
                        // Lấy ra những mặt trụ song song với Z và mặt phẳng vuông góc với Z
                        var listcylindricalFace = cylindricalFaces.Where(c => Math.Abs(Math.Abs(c.Axis.Z) - 1) < tolerance).ToList();
                        var listPlanarFace = planarFacesOfSolid.Where(f => Math.Abs(Math.Abs(f.FaceNormal.Z) - 1) < tolerance).ToList();
                        if (listcylindricalFace.Count > 0)
                        {
                            if (listPlanarFace.Count == 2)
                            {
                                CylindricalFace cylindricalFace = listcylindricalFace.FirstOrDefault();
                                cylinderInfos.Add(CreateCylinderInfo(cylindricalFace, planarFacesOfSolid.Max(f => f.Origin.Z), planarFacesOfSolid.Min(f => f.Origin.Z)));
                            }
                            else
                            {
                                XYZ firstOrigin = new XYZ();
                                var newListFace = new List<CylindricalFace>();
                                for (int i = 0; i < listcylindricalFace.Count; i++)
                                {
                                    if (!listcylindricalFace[i].Origin.IsAlmostEqualTo(firstOrigin, tolerance))
                                    {
                                        newListFace.Add(listcylindricalFace[i]);
                                        firstOrigin = listcylindricalFace[i].Origin;
                                    }
                                }
                                foreach (var face in newListFace)
                                {
                                    var tupleValues = GetHighestAndLowestPointOfFace(face);
                                    if (tupleValues.Max.Z > 0 && tupleValues.Min.Z > 0)
                                    {
                                        double maxZ = tupleValues.Max.Z;
                                        double minZ = tupleValues.Min.Z;

                                        cylinderInfos.Add(CreateCylinderInfo(face, maxZ, minZ));
                                    }
                                }
                            }
                        }
                    }
                    else if (cylindricalFaces.Count == 0 && planarFacesOfSolid.Count > 0)
                    {
                        planarFacesOfElement.AddRange(tuple.Item1);
                    }
                }

                if (planarFacesOfElement.Count > 0)
                {
                    AddMesh(meshes, planarFacesOfElement);
                    foreach (PlanarFace p in planarFacesOfElement)
                    {
                        //Mesh mesh = p.Triangulate();
                        //meshes.Add(mesh);
                        // Kiểm tra xem facenormal của planarface có gần vuông góc với Z không
                        if (IsFaceParallelToZ(p.FaceNormal))
                        {
                            Mesh mesh = p.Triangulate();
                            AddMeshTriangles(triangles, mesh);
                        }
                    }
                }
            }
            //var result = GroupFacesBySharedVertices(planarFacesParallelToZ);
            var result = GroupMeshTrianglesBySharedVertices(triangles);
            result = result.OrderByDescending(x => x.Count).ToList();
            double firstRadius = 0;
            foreach (var group in result)
            {
                //cylinderInfos.Add(GetCylinderInfosFromMeshTriangles(group));
                if (group.Count > 10)
                {
                    var cylinderInfoFromMeshTriangles = GetCylinderInfosFromMeshTriangles(group);
                    if (firstRadius == 0)
                    {
                        firstRadius = cylinderInfoFromMeshTriangles.Radius;
                    }
                    if (IsVaLidCylinderInfo(cylinderInfoFromMeshTriangles, firstRadius))
                    {
                        cylinderInfos.Add(cylinderInfoFromMeshTriangles);
                    }
                }
            }
            return (meshes, cylinderInfos);
        }

        private void AddMesh<T>(List<Mesh> meshes, List<T> faces) where T : Face
        {
            foreach (var f in faces)
            {
                Mesh mesh = f.Triangulate();
                meshes.Add(mesh);
            }
        }

        private bool IsVaLidCylinderInfo(CylinderInfo cylinderInfo, double radius)
        {
            double heightOfCylinder = cylinderInfo.TopPoint.Z - cylinderInfo.BottomPoint.Z;
            // chiều cao của cột trụ lớn hơn 2 lần đường kính cột trụ
            if (heightOfCylinder > radius * 4)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Thêm những triangle mà vuông góc với trục Z vào từ mesh
        /// </summary>
        /// <param name="triangles"></param>
        /// <param name="mesh"></param>
        private void AddMeshTriangles(HashSet<MeshTriangle> triangles, Mesh mesh)
        {
            for (int i = 0; i < mesh.NumTriangles; i++)
            {
                MeshTriangle triangle = mesh.get_Triangle(i);
                XYZ normal = GetNormalFromTriangle(triangle);
                if (IsFaceParallelToZ(normal))
                {
                    triangles.Add(triangle);
                }
            }
        }

        private CylinderInfo GetCylinderInfosFromMeshTriangles(List<MeshTriangle> group)
        {
            CylinderInfo cylinderInfo = new CylinderInfo();

            List<XYZ> points = new List<XYZ>();
            XYZ highestPoint = null;
            XYZ lowestPoint = null;

            double maxZ = double.MinValue;
            double minZ = double.MaxValue;

            foreach (var triangle in group)
            {
                var tuple = GetHighestAndLowestZPoint(triangle);
                if (tuple.Max.Z > maxZ)
                {
                    maxZ = tuple.Max.Z;
                    highestPoint = tuple.Max;
                }
                if (tuple.Min.Z < minZ)
                {
                    minZ = tuple.Min.Z;
                    lowestPoint = tuple.Min;
                }

                points.AddRange(GetVerticesOfTriangle(triangle));
            }
            XYZ centerPoint = GetCenterPoint(points);

            cylinderInfo.TopPoint = SetOriginPoint(centerPoint, highestPoint.Z);
            cylinderInfo.BottomPoint = SetOriginPoint(centerPoint, lowestPoint.Z);
            cylinderInfo.Radius = highestPoint.DistanceTo(cylinderInfo.TopPoint);
            if (defaultRadius < cylinderInfo.Radius)
            {
                defaultRadius = cylinderInfo.Radius;
            }

            return cylinderInfo;
        }

        private (XYZ Max, XYZ Min) GetHighestAndLowestZPoint(MeshTriangle triangle)
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
        private XYZ GetNormalFromTriangle(MeshTriangle tri)
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

        private XYZ GetCenterPoint(List<XYZ> points)
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

        private List<List<MeshTriangle>> GroupMeshTrianglesBySharedVertices(HashSet<MeshTriangle> triangles)
        {
            var result = new List<List<MeshTriangle>>();
            var visited = new HashSet<MeshTriangle>();
            var vertexToTriangles = new Dictionary<XYZ, List<MeshTriangle>>(new XYZComparer(tolerance));

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
        /// Hàm dùng để gom nhóm các tam giác có chung đỉnh lại với nhau
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        //private List<List<MeshTriangle>> GroupMeshTrianglesBySharedVertices(List<MeshTriangle> triangles)
        //{
        //    List<List<MeshTriangle>> result = new List<List<MeshTriangle>>();
        //    HashSet<MeshTriangle> visited = new HashSet<MeshTriangle>();

        //    foreach (var triangle in triangles)
        //    {
        //        if (visited.Contains(triangle))
        //            continue;

        //        List<MeshTriangle> group = new List<MeshTriangle>();
        //        List<MeshTriangle> toCheck = new List<MeshTriangle> { triangle };

        //        for (int i = 0; i < toCheck.Count; i++)
        //        {
        //            var current = toCheck[i];
        //            if (visited.Contains(current))
        //                continue;

        //            visited.Add(current);
        //            group.Add(current);

        //            var currentVertices = GetVerticesOfTriangle(current);

        //            foreach (var other in triangles)
        //            {
        //                if (visited.Contains(other) || toCheck.Contains(other))
        //                    continue;

        //                var otherVertices = GetVerticesOfTriangle(other);
        //                if (HasCommonVertex(currentVertices, otherVertices))
        //                {
        //                    toCheck.Add(other);
        //                }
        //            }
        //        }

        //        result.Add(group);
        //    }

        //    return result;
        //}

        //private List<List<PlanarFace>> GroupFacesBySharedVertices(List<PlanarFace> faces)
        //{
        //    List<List<PlanarFace>> result = new List<List<PlanarFace>>();
        //    HashSet<PlanarFace> visited = new HashSet<PlanarFace>();

        //    foreach (var face in faces)
        //    {
        //        if (visited.Contains(face))
        //            continue;

        //        List<PlanarFace> group = new List<PlanarFace>();
        //        List<PlanarFace> toCheck = new List<PlanarFace> { face };

        //        for (int i = 0; i < toCheck.Count; i++)
        //        {
        //            var current = toCheck[i];
        //            if (visited.Contains(current))
        //                continue;

        //            visited.Add(current);
        //            group.Add(current);

        //            var currentVertices = GetVerticesOfFace(current);

        //            foreach (var other in faces)
        //            {
        //                if (visited.Contains(other) || toCheck.Contains(other))
        //                    continue;

        //                var otherVertices = GetVerticesOfFace(other);
        //                if (HasCommonVertex(currentVertices, otherVertices))
        //                {
        //                    toCheck.Add(other);
        //                }
        //            }
        //        }

        //        result.Add(group);
        //    }

        //    return result;
        //}

        private bool HasCommonVertex(List<XYZ> vertsA, List<XYZ> vertsB)
        {
            foreach (var a in vertsA)
            {
                foreach (var b in vertsB)
                {
                    if (a.IsAlmostEqualTo(b, tolerance))
                        return true;
                }
            }
            return false;
        }

        //private bool HasCommonVertex(IList<XYZ> verts1, IList<XYZ> verts2)
        //{
        //    foreach (var v1 in verts1)
        //    {
        //        foreach (var v2 in verts2)
        //        {
        //            if (v1.IsAlmostEqualTo(v2))
        //                return true;
        //        }
        //    }
        //    return false;
        //}

        /// <summary>
        /// Hàm dùng để lấy ra danh sách các điểm thuộc 1 triangle
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        private List<XYZ> GetVerticesOfTriangle(MeshTriangle triangle)
        {
            return new List<XYZ>
            {
                triangle.get_Vertex(0),
                triangle.get_Vertex(1),
                triangle.get_Vertex(2)
            };
        }

        //private IList<XYZ> GetVerticesOfFace(PlanarFace planarFace)
        //{
        //    IList<XYZ> vertices = new List<XYZ>();
        //    Mesh mesh = planarFace.Triangulate();
        //    if (mesh == null || mesh.Vertices.Count == 0)
        //    {
        //        TaskDialog.Show("Noti", "Face không có mesh hoặc không có đỉnh");
        //    }
        //    else
        //    {
        //        vertices = mesh.Vertices;
        //    }
        //    return vertices;
        //}

        /// <summary>
        ///  Hàm kiểm tra xem face có song song với trục Z hay không
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private bool IsFaceParallelToZ(XYZ normal, double tolerance = 1e-3)
        {
            return Math.Abs(normal.DotProduct(XYZ.BasisZ)) < tolerance;
        }

        /// <summary>
        /// Tạo ra 1 CylinderInfo từ các thông số của 1 CylindricalFace
        /// </summary>
        /// <param name="face"></param>
        /// <param name="maxZ">Giá trị Z cao nhất của CylindricalFace</param>
        /// <param name="minZ">Giá trị Z thấp nhất của CylindricalFace</param>
        /// <returns></returns>
        private CylinderInfo CreateCylinderInfo(CylindricalFace face, double maxZ, double minZ)
        {
            CylinderInfo cylinderInfo = new CylinderInfo();

            cylinderInfo.Radius = GetRadius(face);
            cylinderInfo.TopPoint = SetOriginPoint(face.Origin, maxZ);
            cylinderInfo.BottomPoint = SetOriginPoint(face.Origin, minZ);

            return cylinderInfo;
        }

        /// <summary>
        /// Lấy giá trị Z cao nhất và thấp nhất của 1 CylindricalFace
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private (XYZ Max, XYZ Min) GetHighestAndLowestPointOfFace(Face face)
        {
            XYZ highestPoint = new XYZ();
            XYZ lowestPoint = new XYZ();

            // Lấy tất cả các điểm từ Face bằng cách tessellate nó.

            Mesh mesh = face.Triangulate();

            if (mesh == null || mesh.Vertices.Count == 0)
            {
                return (highestPoint, lowestPoint); // Face không có mesh hoặc không có đỉnh
            }

            // Khởi tạo điểm cao nhất và thấp nhất với đỉnh đầu tiên
            highestPoint = mesh.Vertices[0];
            lowestPoint = mesh.Vertices[0];

            // Duyệt qua tất cả các đỉnh của mesh để tìm Z_max và Z_min
            foreach (XYZ vertex in mesh.Vertices)
            {
                if (vertex.Z > highestPoint.Z)
                {
                    highestPoint = vertex;
                }
                if (vertex.Z < lowestPoint.Z)
                {
                    lowestPoint = vertex;
                }
            }

            return (highestPoint, lowestPoint);
        }

        /// <summary>
        /// Lấy bán kính của 1 CylindricalFace
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private double GetRadius(CylindricalFace face)
        {
            CylindricalSurface s = face.GetSurface() as CylindricalSurface;
            double radius = s.Radius;
            return radius;
        }

        /// <summary>
        /// Chiếu 2 điểm của 1 line lên 1 mặt phẳng, mục đích để tạo ra 1 line mới nằm trên plane
        /// sử dụng trong trường hợp line cũ nằm rất gần plane nhưng do sai số nên không nằm trên plane đó
        /// </summary>
        /// <param name="line"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        private Line ProjectLineOntoSketchPlane(Line line, Plane plane)
        {
            XYZ p1 = line.GetEndPoint(0);
            XYZ p2 = line.GetEndPoint(1);

            // Tính khoảng cách từ 2 điểm đến mặt phẳng
            double d1 = (p1 - plane.Origin).DotProduct(plane.Normal);
            double d2 = (p2 - plane.Origin).DotProduct(plane.Normal);

            p1 = p1 - d1 * plane.Normal;
            p2 = p2 - d2 * plane.Normal;
            return Line.CreateBound(p1, p2);
        }

        /// <summary>
        /// Vẽ modelline mới từ 2 điểm bất kỳ
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        private void CreateModelLine(Document doc, XYZ point1, XYZ point2)
        {
            using (Transaction trans = new Transaction(doc, "Create Model Line with Auto Plane"))
            {
                trans.Start();
                ModelCurve modelCurve = null;

                XYZ pt1 = transform.OfPoint(point1);
                XYZ pt2 = transform.OfPoint(point2);

                XYZ newVector = XYZ.BasisZ.Multiply(1);
                XYZ p1 = pt1.Add(newVector);
                XYZ p2 = pt2.Add(newVector);

                Line line = Line.CreateBound(p1, p2);
                XYZ direction = (p2 - p1).Normalize();

                bool isParallelToX = Math.Abs(direction.DotProduct(XYZ.BasisX)) > 0.99;
                bool isParallelToY = Math.Abs(direction.DotProduct(XYZ.BasisY)) > 0.99;
                bool isParallelToZ = Math.Abs(direction.DotProduct(XYZ.BasisZ)) > 0.99;

                Plane plane;

                if (isParallelToX)
                {
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisY, p1);
                }
                else if (isParallelToY)
                {
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, p1);
                }
                else if (isParallelToZ)
                {
                    plane = Plane.CreateByNormalAndOrigin(XYZ.BasisX, p1);
                }
                else
                {
                    XYZ normal = direction.CrossProduct(XYZ.BasisZ).Normalize();
                    plane = Plane.CreateByNormalAndOrigin(normal, p1);
                }
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                if (isParallelToX || isParallelToY || isParallelToZ)
                {
                    Line snappedLine = ProjectLineOntoSketchPlane(line, plane);
                    if (snappedLine != null)
                    {
                        modelCurve = doc.Create.NewModelCurve(snappedLine, sketchPlane);
                    }
                    else
                    {
                        TaskDialog.Show("Lỗi", "Line không nằm gần SketchPlane. Không thể vẽ.");
                    }
                }
                else
                {
                    modelCurve = doc.Create.NewModelCurve(line, sketchPlane);
                }
                //Random random = new Random();

                //// Tạo giá trị RGB ngẫu nhiên từ 0 đến 255
                //byte red = (byte)random.Next(0, 256);
                //byte green = (byte)random.Next(0, 256);
                //byte blue = (byte)random.Next(0, 256);

                //OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                //ogs.SetProjectionLineColor(new Color(red, green, blue));

                //doc.ActiveView.SetElementOverrides(modelCurve.Id, ogs);

                trans.Commit();
            }
        }

        /// <summary>
        /// Tính chiều dài thang xoắn dựa vào danh sách các CylinderInfo
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private double CalculateSpiralRailingLength(List<List<CylinderInfo>> result, Document doc, UIDocument uiDoc)
        {
            double length = 0;
            var group = result.FirstOrDefault();

            int num = group.Count;
            for (int i = 2; i < num; i += 2)
            {
                Arc arc = Arc.Create(group[i - 2].TopPoint, group[i].TopPoint, group[i - 1].TopPoint);
                length += arc.Length;
                CreateModelArcFrom3Points(uiDoc, doc, group[i - 2].TopPoint, group[i].TopPoint, group[i - 1].TopPoint);
            }
            if (num % 2 == 0)
            {
                length += group[num - 1].TopPoint.DistanceTo(group[num - 2].TopPoint);
                CreateModelArcFrom3Points(uiDoc, doc, group[num - 3].TopPoint, group[num - 1].TopPoint, group[num - 2].TopPoint);
            }
            TaskDialog.Show("Nofi", "The length of railing is : " + length.ToString());
            return length;
        }

        /// <summary>
        /// Nhóm các điểm thuộc cùng 1 mặt phẳng lại với nhau
        /// </summary>
        /// <param name="cylinderInfos"></param>
        /// <returns></returns>
        private List<List<CylinderInfo>> GroupPointsOnSamePlane(List<CylinderInfo> cylinderInfos)
        {
            List<List<CylinderInfo>> result = new List<List<CylinderInfo>>();
            for (int i = 0; i < cylinderInfos.Count - 2; i++)
            {
                for (int j = i + 1; j < cylinderInfos.Count - 1; j++)
                {
                    for (int k = j + 1; k < cylinderInfos.Count; k++)
                    {
                        XYZ p1 = cylinderInfos[i].TopPoint;
                        XYZ p2 = cylinderInfos[j].TopPoint;
                        XYZ newp2 = new XYZ(p2.X, p2.Y, p2.Z + 1);
                        XYZ p3 = cylinderInfos[k].TopPoint;

                        // Tính vector pháp tuyến của mặt phẳng đi qua 3 điểm
                        XYZ v1 = newp2 - p1;
                        XYZ v2 = p3 - p1;
                        XYZ normal = v1.CrossProduct(v2);
                        if (normal.IsZeroLength())
                            continue; // 3 điểm thẳng hàng, không tạo được mặt phẳng

                        normal = normal.Normalize();

                        // Nếu pháp tuyến vuông góc với trục Z, nghĩa là mặt phẳng song song với trục Z
                        XYZ zAxis = XYZ.BasisZ;
                        double dot = Math.Abs(normal.DotProduct(zAxis));
                        if (dot > cosineAngleTolerance)
                            continue;

                        // Tạo mặt phẳng
                        Plane plane = Plane.CreateByNormalAndOrigin(normal, p1);

                        // Gom các điểm thuộc mặt phẳng này
                        List<CylinderInfo> group = new List<CylinderInfo>();
                        foreach (var pt in cylinderInfos)
                        {
                            if (IsPointOnPlane(plane, pt.TopPoint))
                            {
                                group.Add(pt);
                            }
                        }

                        // Nếu nhóm này đủ 3 điểm và chưa bị thêm, thì thêm vào kết quả
                        if (group.Count >= 3)
                        {
                            // Kiểm tra trùng nhóm (dựa vào trùng điểm)
                            bool isDuplicate = result.Any(g => group.All(p => g.Contains(p)));
                            if (!isDuplicate)
                            {
                                result.Add(group);
                            }
                        }
                        //Trường hợp tồn tại group 3:

                        //Nếu tồn tại chỉ 1 group 2 nữa : ko xử lý gì
                        // Nếu tồn tại 2 group 2 :

                        // Nếu group1(A,B) và group2(B,C) mà trong đó B chung , A và C đều thuộc 2 group 3 khác
                        // tức là có 1 điểm lẻ nằm ở giữa 2 group 3 khác nhau thì xử lý điểm lẻ đó với từng group 3
                        // Nếu chỉ có A thuộc group 3 khác, còn C thì không thì ko xử lý gì

                        //Trường hợp không tồn tại group 3:
                        // Xét trường hợp các điểm có tạo thành 1 hàng thẳng hay chéo (thẳng thì cho top bằng nhau, chéo thì kéo dài tia để tìm giao điểm)
                        // Nếu chỉ tồn tại 3 group 2 mà group 1 vuông góc với group 3
                        // Nếu chỉ tồn tại 2 group 2 trong đó có điểm B chung thì 2 điểm gần nhau nhất tạo thành 1 mặt phẳng
                        //if (group.Count == 2)
                        //{
                        //}
                    }
                }
            }

            return result;
        }

        private bool ValidateInstanceSpacing(List<CylinderInfo> pointsAsDiagonalLine, CylinderInfo remainCylinderInfo)
        {
            XYZ remainPoint = SetOriginPoint(remainCylinderInfo.TopPoint, 0);

            XYZ firstPoint = SetOriginPoint(pointsAsDiagonalLine[0].TopPoint, 0);
            XYZ nextPoint = SetOriginPoint(pointsAsDiagonalLine[1].TopPoint, 0);
            double distance = firstPoint.DistanceTo(nextPoint);

            // Khoảng cách từ điểm thừa đến điểm gần nhất trong danh sách
            double minDistance = pointsAsDiagonalLine.Min(p => SetOriginPoint(p.TopPoint, 0).DistanceTo(remainPoint));
            if (Math.Abs(distance - minDistance) < remainCylinderInfo.Radius * 2)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Xử lý trường hợp có 1 danh sách trụ thẳng hàng và có 1 trụ thừa
        /// </summary>
        /// <param name="newResult"></param>
        /// <param name="remainCylinderInfos"></param>
        /// <param name="pointsAsStraightLine"></param>
        private void HandleSinglePointOutOfStraightLine(List<CylinderInfo> remainCylinderInfos, List<CylinderInfo> pointsAsStraightLine)
        {
            CylinderInfo remainCylinderInfo = remainCylinderInfos.FirstOrDefault();
            CylinderInfo firstCylinderInfo = pointsAsStraightLine.FirstOrDefault();

            if (remainCylinderInfo.TopPoint.Z < firstCylinderInfo.TopPoint.Z
                && remainCylinderInfo.BottomPoint.Z > firstCylinderInfo.BottomPoint.Z)
            {
                remainCylinderInfo.TopPoint = SetOriginPoint(remainCylinderInfo.TopPoint, firstCylinderInfo.TopPoint.Z);
                pointsAsStraightLine.Add(remainCylinderInfo);
                SortInstancesAlongLine(pointsAsStraightLine);
            }
        }

        /// <summary>
        /// Xử lý trường hợp có 1 danh sách trụ chéo và có 1 trụ thừa
        /// </summary>
        /// <param name="newResult"></param>
        /// <param name="remainCylinderInfo"></param>
        /// <param name="pointsAsDiagonalLine"></param>
        private void HandleSinglePointOutOfDiagonalLine(List<List<CylinderInfo>> newResult, List<CylinderInfo> remainCylinderInfos, List<CylinderInfo> pointsAsDiagonalLine)
        {
            // Trường hợp group thừa 1 trụ nằm ngoài khoảng của group chéo
            // kéo dài tia tạo bởi 2 trụ trong group chéo, nếu đi qua trụ thừa mà cao hơn thì lấy tại điểm cao hơn
            // nếu kéo dài tia mà cắt trụ thì lấy tại điểm thuộc tia và có chiều cao bằng trụ thừa
            var e = remainCylinderInfos.FirstOrDefault();

            double ze = e.TopPoint.Z;
            double maxZC = pointsAsDiagonalLine.Max(x => x.TopPoint.Z);
            double minZC = pointsAsDiagonalLine.Min(x => x.TopPoint.Z);

            double zOnline = GetZOnLine(pointsAsDiagonalLine[0].TopPoint, pointsAsDiagonalLine[1].TopPoint, e.TopPoint);
            XYZ C = new XYZ();
            if (ze > minZC && ze < maxZC) // C cùng đường thẳng với E
            {
                C = new XYZ(e.TopPoint.X, e.TopPoint.Y, zOnline);
            }
            else if (ze == minZC || ze == maxZC)
            {
                double targetZ = ze == minZC ? minZC : maxZC;

                var otherPointsAsStraightLine = new List<CylinderInfo>
                {
                    e,
                    pointsAsDiagonalLine.FirstOrDefault(c => c.TopPoint.Z.Equals(targetZ))
                };

                if (otherPointsAsStraightLine.Count > 0)
                {
                    newResult.Add(otherPointsAsStraightLine);
                }
            }
            else
            {
                if (zOnline < ze)
                {
                    // dùng phương trình đường thẳng để tìm ra điểm C thuộc đường thẳng AB và có cao độ là E
                    C = GetPointOnLine(pointsAsDiagonalLine[0].TopPoint, pointsAsDiagonalLine[1].TopPoint, e.TopPoint);
                }
                else if (zOnline > maxZC)
                {
                    if (ValidateInstanceSpacing(pointsAsDiagonalLine, e))
                    {
                        C = new XYZ(e.TopPoint.X, e.TopPoint.Y, zOnline);
                    }
                    else
                    {
                        C = GetPointOnLine(pointsAsDiagonalLine[0].TopPoint, pointsAsDiagonalLine[1].TopPoint, e.TopPoint);
                    }
                }
                else if (zOnline > ze && zOnline < minZC)
                {
                    // Tạo ra điểm C với tọa độ của E nhưng chỉnh cao độ lên bằng với giao điểm
                    C = new XYZ(e.TopPoint.X, e.TopPoint.Y, zOnline);
                }
            }
            if (!C.IsAlmostEqualTo(XYZ.Zero))
            {
                CylinderInfo cylinderInfo = new CylinderInfo();
                cylinderInfo.TopPoint = C;
                List<CylinderInfo> newPointsOnStraightLine = new List<CylinderInfo>();
                // Nếu C cùng chiều cao nhưng khác x hoặc y với E (C không thuộc trụ chứa E)
                if (CompareDouble(C.Z, e.TopPoint.Z) && (!CompareDouble(C.X, e.TopPoint.X) || !CompareDouble(C.Y, e.TopPoint.Y)))
                {
                    newPointsOnStraightLine.Add(e);
                    newPointsOnStraightLine.Add(cylinderInfo);
                    if (C.Z < minZC)
                    {
                        pointsAsDiagonalLine.Insert(0, cylinderInfo);
                    }
                    else if (C.Z > maxZC)
                    {
                        pointsAsDiagonalLine.Add(cylinderInfo);
                    }
                }
                else
                {
                    if (C.Z < minZC)
                    {
                        pointsAsDiagonalLine.Insert(0, cylinderInfo);
                    }
                    else if (C.Z > maxZC)
                    {
                        pointsAsDiagonalLine.Add(cylinderInfo);
                    }
                }
                if (newPointsOnStraightLine.Count > 0)
                {
                    newResult.Add(newPointsOnStraightLine);
                }
            }
            newResult.Add(pointsAsDiagonalLine);
        }

        /// <summary>
        /// Xử lý trường hợp trong group tồn tại hả danh sách hàng chéo và danh sách các trụ thẳng hàng
        /// </summary>
        /// <param name="newResult"></param>
        /// <param name="pointsAsDiagonalLine"></param>
        /// <param name="pointsAsStraightLine"></param>
        private void HandleDiagonalAndStraightLines(List<List<CylinderInfo>> newResult, List<CylinderInfo> pointsAsDiagonalLine, List<CylinderInfo> pointsAsStraightLine)
        {
            // kiểm tra có điểm chung không
            bool haveCommonItem = false;
            foreach (var t in pointsAsStraightLine)
            {
                foreach (var c in pointsAsDiagonalLine)
                {
                    if (Math.Abs(t.TopPoint.X - c.TopPoint.X) < tolerance
                        && Math.Abs(t.TopPoint.Y - c.TopPoint.Y) < tolerance)
                    {
                        haveCommonItem = true;
                        break;
                    }
                }
                if (haveCommonItem)
                    break;
            }
            // Nếu không có điểm chung thì tạo 1 điểm C mới là giao của 2 tia đường thẳng và đường chéo
            if (haveCommonItem == false)
            {
                double zT = pointsAsStraightLine.First().TopPoint.Z;
                double maxZC = pointsAsDiagonalLine.Max(c => c.TopPoint.Z);
                double minZC = pointsAsDiagonalLine.Min(c => c.TopPoint.Z);

                XYZ C = GetPointOnLine(pointsAsDiagonalLine[0].TopPoint, pointsAsDiagonalLine[1].TopPoint, pointsAsStraightLine.First().TopPoint);
                if (C != null)
                {
                    CylinderInfo cylinderInfo = new CylinderInfo();
                    cylinderInfo.TopPoint = C;

                    if (zT < minZC)
                    {
                        pointsAsStraightLine.Add(cylinderInfo);
                        pointsAsDiagonalLine.Insert(0, cylinderInfo);
                    }
                    else if (zT > maxZC)
                    {
                        pointsAsStraightLine.Insert(0, cylinderInfo);
                        pointsAsDiagonalLine.Add(cylinderInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Xử lý trụ thừa. trụ không tạo thành 1 mặt phẳng với ít nhất 2 trụ khác
        /// </summary>
        /// <param name="newResult"></param>
        /// <param name="remainingcylinderInfos"></param>
        /// <param name="usedCylinderInfo"></param>
        private void HandleRedundantPoint(List<List<CylinderInfo>> newResult, List<CylinderInfo> remainingcylinderInfos, List<CylinderInfo> usedCylinderInfo)
        {
            foreach (var cylinderInfoA in remainingcylinderInfos)
            {
                XYZ A = cylinderInfoA.TopPoint;
                if (!usedCylinderInfo.Contains(cylinderInfoA))
                {
                    usedCylinderInfo.Add(cylinderInfoA);
                    XYZ B = null;
                    XYZ newA = null;
                    double minDist = double.MaxValue;

                    List<CylinderInfo> closestGroup = null;
                    Plane groupPlane = null;

                    foreach (var group in newResult)
                    {
                        for (int i = 0; i < group.Count - 2; i++)
                        {
                            XYZ thirdPoint = new XYZ(group[i + 2].TopPoint.X, group[i + 2].TopPoint.Y, 0);
                            Plane plane = Plane.CreateByThreePoints(group[i].TopPoint, group[i + 1].TopPoint, thirdPoint);
                            foreach (var pt in group)
                            {
                                double dist = A.DistanceTo(pt.TopPoint);
                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    B = pt.TopPoint;
                                    closestGroup = group;
                                    groupPlane = plane;

                                    newA = new XYZ(A.X, A.Y, B.Z);
                                }
                            }
                        }
                    }
                    if (B == null || groupPlane == null)
                        continue;

                    XYZ E = GetProjectedPoint(groupPlane, newA);
                    CylinderInfo newCylinderInfo = new CylinderInfo();
                    newCylinderInfo.TopPoint = E;

                    cylinderInfoA.TopPoint = newA;

                    if (!closestGroup.Any(p => p.Equals(newCylinderInfo)))
                    {
                        closestGroup.Add(newCylinderInfo);
                        // Sắp xếp theo thứ tự gần new A nhất
                        closestGroup.Sort((a, b) => newA.DistanceTo(a.TopPoint).CompareTo(newA.DistanceTo(b.TopPoint)));
                    }

                    List<CylinderInfo> newGroup = new List<CylinderInfo> { cylinderInfoA, newCylinderInfo };
                    // Xây mặt phẳng từ A, E và pháp tuyến mới

                    XYZ newNormal = (B - E).Normalize();
                    Plane aePlane = Plane.CreateByNormalAndOrigin(newNormal, newA);

                    // Tìm kiếm những điểm lẻ khác mà thuộc mặt phẳng AE
                    foreach (var info in remainingcylinderInfos.Where(p => !usedCylinderInfo.Contains(p)).ToList())
                    {
                        if (info.Equals(cylinderInfoA))
                            continue;

                        // Trường hợp nếu có thêm điểm thừa khác thuộc mặt phẳng thì kiểm trả xem 3 điểm đó thẳng hàng không
                        // thẳng hàng thì thêm vào, còn không chỉ đó là 3 điểm lệch nhau(trường hợp này chưa có model nào giống vậy để xử lý)
                        if (IsPointOnPlane(aePlane, info.TopPoint))
                        {
                            newGroup.Add(info);
                            usedCylinderInfo.Add(info);
                        }
                    }
                    SortInstancesAlongLine(newGroup);
                    newResult.Add(newGroup);
                }
            }
        }

        /// <summary>
        /// Nhóm các hình trụ có thành các nhóm nhỏ để tính khoảng cách, các nhóm nhỏ bao gồm các nhóm chéo, nhóm thẳng
        /// </summary>
        /// <param name="cylinderInfos"></param>
        /// <returns></returns>
        private List<List<CylinderInfo>> GroupPointsForDistanceCalculation(List<CylinderInfo> cylinderInfos)
        {
            var result = GroupPointsOnSamePlane(cylinderInfos);

            List<List<CylinderInfo>> newResult = new List<List<CylinderInfo>>();

            // 1 group là 1 mặt phẳng, 1 mặt phẳng yêu cầu có ít nhất 3 trụ
            foreach (var group in result)
            {
                SortInstancesAlongLine(group);
                var newGroup = CloneList(group);

                List<CylinderInfo> pointsAsDiagonalLine = GroupPointsAsDiagonalLine(newGroup);
                List<CylinderInfo> pointsAsStraightLine = GroupPointsAsStraightLine(newGroup, pointsAsDiagonalLine);

                HashSet<CylinderInfo> excluded = new HashSet<CylinderInfo>(pointsAsStraightLine.Concat(pointsAsDiagonalLine));

                List<CylinderInfo> remainCylinderInfos = newGroup.Where(x => !excluded.Contains(x)).ToList();

                // Xử lý trường hợp có 1 điểm thừa không thuộc danh sách thẳng
                if (remainCylinderInfos.Count == 1 && pointsAsStraightLine.Count > 0)
                {
                    HandleSinglePointOutOfStraightLine(remainCylinderInfos, pointsAsStraightLine);
                }

                // Xử lý trường hợp có 1 điểm thừa không thuộc danh sách chéo
                if (remainCylinderInfos.Count == 1 && pointsAsDiagonalLine.Count > 0) // có tồn tại 1 điểm thừa
                {
                    HandleSinglePointOutOfDiagonalLine(newResult, remainCylinderInfos, pointsAsDiagonalLine);
                }
                // Trường hợp không tồn tại điểm thừa
                // Tồn tại cả danh sách thẳng và chéo
                if (pointsAsStraightLine.Count > 0 && pointsAsDiagonalLine.Count > 0)
                {
                    HandleDiagonalAndStraightLines(newResult, pointsAsDiagonalLine, pointsAsStraightLine);
                }

                if (pointsAsStraightLine.Count > 0)
                {
                    newResult.Add(pointsAsStraightLine);
                }
                if (!newResult.Contains(pointsAsDiagonalLine) && pointsAsDiagonalLine.Count > 0)
                {
                    newResult.Add(pointsAsDiagonalLine);
                }
            }

            // Trường hợp thừa 1-2 điểm không tạo thành 1 mặt phẳng
            HashSet<CylinderInfo> allGroupedPoints = new HashSet<CylinderInfo>(result.SelectMany(g => g));
            var remainingcylinderInfos = cylinderInfos.Where(p => !allGroupedPoints.Any(q => q.Equals(p))).ToList();
            List<CylinderInfo> usedCylinderInfo = new List<CylinderInfo>();
            if (remainingcylinderInfos.Count > 0)
            {
                HandleRedundantPoint(newResult, remainingcylinderInfos, usedCylinderInfo);
            }

            return newResult;
        }

        /// <summary>
        /// So sánh 2 số double
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        private bool CompareDouble(double d1, double d2)
        {
            return Math.Abs(d1 - d2) < tolerance;
        }

        /// <summary>
        /// Tìm ra 1 điểm thuộc đường thằng tạo bởi A và B và có cao độ Z bằng cao độ của E
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="E"></param>
        /// <returns></returns>
        private XYZ GetPointOnLine(XYZ A, XYZ B, XYZ E)
        {
            double zE = E.Z;
            double dZ = B.Z - A.Z;

            if (Math.Abs(dZ) < tolerance)
            {
                return Math.Abs(zE - B.Z) < tolerance ? E : null;
            }
            double t = (zE - A.Z) / dZ;
            double x = A.X + (B.X - A.X) * t;
            double y = A.Y + (B.Y - A.Y) * t;
            double z = zE;

            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Tìm chiều cao của điểm thuộc AB và có tọa độ x,y của E
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="E"></param>
        /// <returns></returns>
        private double GetZOnLine(XYZ A, XYZ B, XYZ E)
        {
            // Trường hợp E hơi lệch tý, không thuộc mặt phẳng đi qua AB và song song với trục Z thì phải tạo 1 điểm E mới nằm trên mặt phẳng

            // C là điểm giống A nhưng có Z = 0, để tạo với A và B thành 1 mặt phẳng song song với Z
            XYZ C = SetOriginPoint(A, 0);
            Plane plane = Plane.CreateByThreePoints(A, B, C);
            XYZ newEOnPlane = GetProjectedPoint(plane, E);

            double t;

            if (Math.Abs(B.X - A.X) > tolerance)
            {
                t = (newEOnPlane.X - A.X) / (B.X - A.X);
            }
            else if (Math.Abs(B.Y - A.Y) > tolerance)
            {
                t = (newEOnPlane.Y - A.Y) / (B.Y - A.Y);
            }
            else
            {
                // Đường AB thẳng đứng (X và Y không đổi)
                // Kiểm tra xem E có cùng X, Y không
                if (Math.Abs(newEOnPlane.X - A.X) < tolerance && Math.Abs(newEOnPlane.Y - A.Y) < tolerance)
                {
                    return newEOnPlane.Z;
                }
                else
                {
                    // Không nằm trên đường thẳng
                    return 0;
                }
            }

            // Tính Z trên đường thẳng tại vị trí t
            return A.Z + (B.Z - A.Z) * t;
        }

        /// <summary>
        /// Nhóm các điểm có toppoint bằng nhau hoặc bottom point bằng nhau
        /// </summary>
        /// <param name="group"></param>
        /// <param name="pointsAsDiagonalLine"></param>
        /// <returns></returns>
        private List<CylinderInfo> GroupPointsAsStraightLine(List<CylinderInfo> group, List<CylinderInfo> pointsAsDiagonalLine)
        {
            var result = new List<CylinderInfo>();
            CylinderInfo first = null;
            int indexFirst = 0;
            int indexLast = 0;
            CylinderInfo last = null;

            for (int i = 0; i < group.Count - 1; i++)
            {
                for (int j = i + 1; j < group.Count; j++)
                {
                    if (ArePointsColinear(group[i], group[j], group[j].Radius))
                    {
                        if (first == null)
                        {
                            first = group[i];
                            indexFirst = i;
                        }
                        last = group[j];
                        indexLast = j;
                    }
                }
                if (first != null && last != null)
                {
                    break;
                }
            }
            List<CylinderInfo> subList = new List<CylinderInfo>();
            if (indexLast > 0)
            {
                subList = group.GetRange(indexFirst, indexLast - indexFirst + 1);
            }
            //if (subList.Count > 2)
            if (subList.Count >= 2)
            {
                result.AddRange(subList);
                // Kiểm tra xem hàng chéo có chung trụ với hàng thẳng không, để khi thay đổi chiều cao hàng thẳng thì không ảnh hưởng đến hàng chéo
                if (pointsAsDiagonalLine.Count > 0)
                {
                    ChangeGroupPointsAsDiagonalLine(pointsAsDiagonalLine, result);
                }

                double topZ = result.Max(c => c.TopPoint.Z);
                foreach (var c in result)
                {
                    c.TopPoint = SetOriginPoint(c.TopPoint, topZ);
                }
                // set lại origin của tất cả bằng với điểm cao nhất

                SortInstancesAlongLine(result);
                return result;
            }
            return result;
        }

        /// <summary>
        /// Sắp xếp các CylinderInfo theo thứ tự
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        //private List<CylinderInfo> SortInstancesAlongLine(List<CylinderInfo> instances)
        //{
        //    if (instances == null || instances.Count < 2)
        //        return instances;

        //    // Gốc là điểm đầu tiên
        //    XYZ origin = instances[0].TopPoint;

        //    // Dùng điểm thứ hai để xác định hướng
        //    XYZ direction = (instances[1].TopPoint - origin).Normalize();

        //    // Tính "t" cho từng instance rồi sắp xếp
        //    var sorted = instances
        //        .Select(inst => new
        //        {
        //            Instance = inst,
        //            T = (inst.TopPoint - origin).DotProduct(direction)
        //        })
        //        .OrderBy(x => x.T)
        //        .Select(x => x.Instance)
        //        .ToList();

        //    return sorted;
        //}

        private void SortInstancesAlongLine(List<CylinderInfo> instances)
        {
            if (instances == null || instances.Count < 2)
                return;

            XYZ origin = instances[0].TopPoint;
            XYZ direction = (instances[1].TopPoint - origin).Normalize();

            // Sắp xếp trực tiếp tại chỗ
            instances.Sort((a, b) =>
            {
                double tA = (a.TopPoint - origin).DotProduct(direction);
                double tB = (b.TopPoint - origin).DotProduct(direction);
                return tA.CompareTo(tB);
            });
        }

        /// <summary>
        /// Tạo ra 1 list CylinderInfo mới từ list CylinderInfo cũ
        /// </summary>
        /// <param name="cylinderInfos"></param>
        /// <returns></returns>
        private List<CylinderInfo> CloneList(List<CylinderInfo> cylinderInfos)
        {
            List<CylinderInfo> list = new List<CylinderInfo>();
            foreach (var c in cylinderInfos)
            {
                list.Add(c.Clone());
            }
            return list;
        }

        /// <summary>
        /// Thay đổi list hàng chéo, nếu list chéo có phần tử chung với list thẳng thì khi list thẳng thay đổi chiều cao Z thì list chéo
        /// cũng bị thay đổi nên sẽ tìm phần tử chung đó, clone ra 1 phần tử mới rồi thay thế nó trong list chéo
        ///  Mục đích để khi thay đổi chiều cao của list thẳng thì điểm chung thuộc list chéo không bị ảnh hưởng
        /// </summary>
        /// <param name="pointsAsDiagonalLine"></param>
        /// <param name="pointsAsStraightLine"></param>
        private void ChangeGroupPointsAsDiagonalLine(List<CylinderInfo> pointsAsDiagonalLine, List<CylinderInfo> pointsAsStraightLine)
        {
            if (pointsAsStraightLine.Any(c => pointsAsDiagonalLine.Contains(c)))
            {
                CylinderInfo commonInstance = null;
                foreach (var p1 in pointsAsStraightLine)
                {
                    foreach (var p2 in pointsAsDiagonalLine)
                    {
                        if (object.ReferenceEquals(p1, p2))
                        {
                            commonInstance = p2;
                            break;
                        }
                    }
                    if (commonInstance != null) break;
                }
                if (commonInstance != null)
                {
                    int index = pointsAsDiagonalLine.IndexOf(commonInstance);
                    var newInstance = commonInstance.Clone();

                    pointsAsDiagonalLine[index] = newInstance; // thay thế trực tiếp
                }
            }
        }

        /// <summary>
        /// Nhóm các điểm tạo thành 1 đường chéo, yêu cầu phải có ít nhất 4 điểm để tạo thành
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private List<CylinderInfo> GroupPointsAsDiagonalLine(List<CylinderInfo> group)
        {
            HashSet<CylinderInfo> mySet = new HashSet<CylinderInfo>();
            var result = new List<CylinderInfo>();
            // Xét thêm trường hợp 2 điểm được coi là tạo thành 1 đường chéo nếu top1 < top2, bot1 < bot2 và đường thẳng tạo bởi top1_top2 phải
            // song song với đường thẳng tạo bởi bot1_bot2

            for (int i = 2; i < group.Count; i++)
            {
                List<CylinderInfo> cylinderInfos = new List<CylinderInfo> { group[i - 2], group[i - 1], group[i] };
                if (ArePointsCollinear(group[i - 2], group[i - 1], group[i]) && IsPointsAsDiagonalLine(cylinderInfos))
                {
                    mySet.Add(group[i]);
                    mySet.Add(group[i - 1]);
                    mySet.Add(group[i - 2]);
                }
            }
            if (mySet.Count >= 3)
            {
                result.AddRange(mySet);
                result.Sort((c1, c2) => c1.TopPoint.Z.CompareTo(c2.TopPoint.Z));

                return result;
            }
            return result;
        }

        /// <summary>
        /// Hàm kiểm tra 3 điểm có tạo thành 1 đường chéo không, tức là top và bottom của trụ tăng dân
        /// </summary>
        /// <param name="cylinderInfos"></param>
        /// <returns></returns>
        private bool IsPointsAsDiagonalLine(List<CylinderInfo> cylinderInfos)
        {
            if (cylinderInfos.Count == 3)
            {
                SortInstancesAlongLine(cylinderInfos);
                //double topC1 = cylinderInfos[0].TopPoint.Z;
                //double topC2 = cylinderInfos[1].TopPoint.Z;
                //double topC3 = cylinderInfos[2].TopPoint.Z;
                double bottomC1 = cylinderInfos[0].BottomPoint.Z;
                double bottomC2 = cylinderInfos[1].BottomPoint.Z;
                double bottomC3 = cylinderInfos[2].BottomPoint.Z;

                double minZ = Math.Min(bottomC1, Math.Min(bottomC2, bottomC3));
                if (minZ != bottomC2)
                {
                    return true;
                }
                return false;

                //if ((topC1 < topC2 && topC2 < topC3 && bottomC1 < bottomC2 && bottomC2 < bottomC3)
                //    || (topC1 > topC2 && topC2 > topC3 && bottomC1 > bottomC2 && bottomC2 > bottomC3))
                //{
                //    return true;
                //}
                //return false;
            }
            return false;
        }

        /// <summary>
        /// Hàm này kiểm tra xem 2 CylinderInfo có chứa 2 điểm có thẳng hàng không, sai số cho phép là 2 lần bán kính
        /// đối với top, còn sai số mặc định đối với bottom
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private bool ArePointsColinear(CylinderInfo c1, CylinderInfo c2, double radius)
        {
            //return (Math.Abs(c1.TopPoint.Z - c2.TopPoint.Z) < radius * 2 || Math.Abs(c1.BottomPoint.Z - c2.BottomPoint.Z) < tolerance);
            return (Math.Abs(c1.TopPoint.Z - c2.TopPoint.Z) < radius * 2 || Math.Abs(c1.BottomPoint.Z - c2.BottomPoint.Z) < radius * 2);
        }

        /// <summary>
        /// Kiểm tra xem 3 điểm có tạo thành 1 đường chéo hay không, sai số cho phép là các vector từ các điểm tạo thành 1 góc nhỏ hơn 5 độ
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="c3"></param>
        /// <returns></returns>
        private bool ArePointsCollinear(CylinderInfo c1, CylinderInfo c2, CylinderInfo c3)
        {
            //double newTolerance = 0.0872; // 5 độ

            XYZ A = c1.TopPoint;
            XYZ B = c2.TopPoint;
            XYZ C = c3.TopPoint;

            XYZ BottomA = c1.BottomPoint;

            XYZ BottomC = c3.BottomPoint;
            double radius = c1.Radius;

            if (Math.Abs(A.Z - B.Z) > radius * 2 && Math.Abs(B.Z - C.Z) > radius * 2
                 && (Math.Abs(BottomA.Z - BottomC.Z) > radius))
            {
                XYZ AB = (B - A).Normalize();
                XYZ AC = (C - A).Normalize();
                //XYZ AD = (D - A).Normalize();

                // Nếu vector AB và AC không cùng phương -> không thẳng hàng
                if (!IsParallel(AB, AC))
                    return false;

                // Nếu vector AB và AD không cùng phương -> không thẳng hàng
                //if (!IsParallel(AB, AD))
                //    return false;

                //| Góc lệch giữa hai vector | Độ dài `CrossProduct` (nếu đã normalize) |
                //        | ------------------------ | ---------------------------------------- |
                //        | 1°                       | ≈ 0.01745 |
                //        | 5°                       | ≈ 0.0872 |
                //        | 10°                    | ≈ 0.1736 |
                //        | 15°                      | ≈ 0.2588 |
                //        | 30°                      | ≈ 0.5 |
                //        | 90°                      | ≈ 1.0 |

                return true;
            }
            return false;
        }

        /// <summary>
        /// Hàm kiểm tra xem 2 vector có song song với nhau hay không
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private bool IsParallel(XYZ v1, XYZ v2)
        {
            var cross = v1.CrossProduct(v2);
            return cross.GetLength() < cosineAngleTolerance;
        }

        /// <summary>
        /// Hàm này dùng để set lại originpoint
        /// </summary>
        /// <param name="p"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private XYZ SetOriginPoint(XYZ p, double z)
        {
            var result = new XYZ(p.X, p.Y, z);
            return result;
        }

        /// <summary>
        /// Hàm này dùng để tìm ra hình chiếu của 1 điểm lên trên 1 plane
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private XYZ GetProjectedPoint(Plane plane, XYZ point)
        {
            XYZ planeOrigin = plane.Origin;
            XYZ planeNormal = plane.Normal.Normalize();
            XYZ pointToOrigin = point - planeOrigin;

            // Tính khoảng cách từ điểm đến mặt phẳng (dọc theo pháp tuyến)
            double distance = pointToOrigin.DotProduct(planeNormal);

            return point - distance * planeNormal;
        }

        /// <summary>
        /// Hàm này kiểm tra xem 1 điểm có nằm trên 1 mặt phẳng không, với sai số cho trước
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool IsPointOnPlane(Plane plane, XYZ point)
        {
            double distance = plane.Normal.DotProduct(point - plane.Origin);
            return Math.Abs(distance) < 0.1;
            //return Math.Abs(distance) < defaultRadius;
        }
    }

    /// <summary>
    /// Lớp dùng để lưu thông tin cần thiết của 1 cột trụ như bán kính, điểm cao nhất và thấp nhất
    /// </summary>
    public class CylinderInfo
    {
        public double Radius { get; set; }
        public XYZ TopPoint { get; set; }
        public XYZ BottomPoint { get; set; }

        public CylinderInfo Clone()
        {
            return new CylinderInfo { Radius = this.Radius, TopPoint = this.TopPoint, BottomPoint = this.BottomPoint };
        }
    }
}