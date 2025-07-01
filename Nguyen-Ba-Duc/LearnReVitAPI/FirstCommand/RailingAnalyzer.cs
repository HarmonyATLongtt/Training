using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using FirstCommand.Support.Constants;
using FirstCommand.Support.DrawOnRevit;
using FirstCommand.Support.FaceHandle;
using FirstCommand.Support.GenericClass;
using FirstCommand.Support.GenericClass.ComparerClass;
using FirstCommand.Support.GeometryHandle;
using FirstCommand.Support.LineHandle;
using FirstCommand.Support.PointHandle;
using FirstCommand.Support.PlaneHandle;
using FirstCommand.Support.SolidHandle;
using FirstCommand.Support.TrianglesHandle;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class RailingAnalyzer : IExternalCommand
    {
        //private const double TOLERANCE = 1e-6;
        //private const double COSINE_ANGLE_TOLERANCE_1_DEGREE = 0.01745; // 1 độ
        //private const double COSINE_ANGLE_TOLERANCE_5_DEGREE = 0.0872; // 5 độ (góc lệch cho phép để vector normal và trục Z được coi là vuông góc)
        private Transform transform = null;

        private double lengthOfLine = 10;
        private double minX = 0;
        private double minY = 0;
        private double maxX = 0;
        private double maxY = 0;
        private bool isRevitLink = false;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                //IList<Reference> selectedRefs = uidoc.Selection.PickObjects(ObjectType.Element, "Chọn các đối tượng");
                //IList<Reference> selectedRefs = uidoc.Selection.PickObjects(ObjectType.PointOnElement, "Chọn các đối tượng");
                IList<Reference> selectedRefs = uidoc.Selection.PickObjects(ObjectType.LinkedElement, "Chọn các đối tượng");

                if (selectedRefs.Count == 0)
                {
                    TaskDialog.Show("Thông báo", "Không có đối tượng nào được chọn.");
                    return Result.Cancelled;
                }
                //List<CylinderInfo> cylinderInfosOfElements = new List<CylinderInfo>();
                List<Element> collectedElements = new List<Element>();
                //List<Mesh> meshes = new List<Mesh>();
                foreach (Reference r in selectedRefs)
                {
                    //ElementId elementId = r.ElementId;
                    //Element element = doc.GetElement(elementId);

                    //Element linkInstance = uidoc.Document.GetElement(r.ElementId);
                    Element element = uidoc.Document.GetElement(r.ElementId);
                    Element targetElement = null;
                    if (element is RevitLinkInstance rli)
                    {
                        //RevitLinkInstance rli = linkInstance as RevitLinkInstance;

                        Document linkDoc = rli.GetLinkDocument();

                        ElementId linkedElemId = r.LinkedElementId;

                        //Element linkedElem = linkDoc.GetElement(linkedElemId);
                        targetElement = linkDoc.GetElement(linkedElemId);
                        transform = rli.GetTransform();
                        isRevitLink = true;
                    }
                    else
                    {
                        targetElement = element;
                    }

                    // Xử lý cụ thể theo từng loại
                    if (targetElement is Railing railing)
                    {
                        // xử lý với railing...
                        HandleRailingCase(doc, railing);
                    }
                    else if (targetElement is Duct || targetElement is Pipe
                        || CheckElementIsPipeFittingOrDuctFitting(targetElement))
                    {
                        collectedElements.Add(targetElement);
                    }
                    //else if (targetElement is Duct duct)
                    //{
                    //    TaskDialog.Show("Loại", "Đây là Duct.\nName: " + duct.Name);
                    //    // xử lý với duct...
                    //}
                    //else if (targetElement is Pipe pipe)
                    //{
                    //    TaskDialog.Show("Loại", "Đây là Pipe.\nName: " + pipe.Name);
                    //    // xử lý với pipe...
                    //}
                    //else if (targetElement is FamilyInstance fi)
                    //{
                    //    var mepModel = fi.MEPModel;
                    //    if (mepModel != null)
                    //    {
                    //        if (fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting)
                    //        {
                    //            // xử lý với duct fitting...
                    //        }
                    //        else if (fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                    //        {
                    //            // xử lý với pipe fitting...
                    //        }
                    //    }
                    //}
                    else // Trường hợp còn lại là meshes hoặc solid
                    {
                        //List<Mesh> meshes = new List<Mesh>();

                        // Trường hợp là solid thì không cần tìm min,max làm gì
                        if (!(targetElement is FamilyInstance))
                        {
                            var (minPoint, maxPoint) = PointUtility.GetBoundingBoxExtents(targetElement);
                            if (minPoint != null && maxPoint != null)
                            {
                                minX = minPoint.X;
                                maxX = maxPoint.X;
                                minY = minPoint.Y;
                                maxY = maxPoint.Y;
                            }
                        }

                        //var tupleValues = GetCylinderInfosFromElements(targetElement, doc);
                        //List<CylinderInfo> cylinderInfos = tupleValues.CylinderInfos;
                        //meshes.AddRange(tupleValues.Meshs);
                        //List<Solid> solids = tupleValues.Solids;

                        var (meshes, cylinderInfos, solids) = GetCylinderInfosFromElements(targetElement, doc);

                        // Trường hợp model là 1 khối thống nhất

                        if (cylinderInfos.Count > 2)
                        {
                            PrepareDataForExecution(cylinderInfos, doc, uidoc, meshes, solids);
                        }
                        //else if (cylinderInfos.Count == 1)
                        //{
                        //    CylinderInfo firstCylinderInfo = cylinderInfos.FirstOrDefault();

                        //    cylinderInfosOfElements.Add(firstCylinderInfo);
                        //}
                        // trường hợp chỉ có 2 trụ
                        else if (cylinderInfos.Count == 2)
                        {
                            HandleInCaseHaveTwoCylinderInfos(doc, cylinderInfos);
                        }
                    }
                }
                // Trường hợp model gồm nhiều element ghép lại
                //if (cylinderInfosOfElements.Count > 0 && cylinderInfosOfElements.Count != 2)
                //{
                //    PrepareDataForExecution(cylinderInfosOfElements, doc, uidoc, meshes);
                //}
                // trường hợp chỉ có 2 trụ
                //else if (cylinderInfosOfElements.Count == 2)
                //{
                //    HandleInCaseHaveTwoCylinderInfos(doc, cylinderInfosOfElements);
                //}

                // Xử lý cho  trường hợp model gồm nhiều element nhỏ ghép lại
                if (collectedElements.Count > 0)
                {
                    HandleCollectedElements(uidoc, doc, collectedElements);
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Người dùng nhấn ESC
                TaskDialog.Show("Thông báo", "Command bị hủy bởi người dùng.");
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }

        /// <summary>
        /// Hàm xử lý cho trường hợp model gồm nhiều element ghép lại
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="collectedElements"></param>
        private void HandleCollectedElements(UIDocument uidoc, Document doc, List<Element> collectedElements)
        {
            List<(Element, List<Solid>)> solidsOfElement = new List<(Element, List<Solid>)>();
            foreach (Element element in collectedElements)
            {
                var solids = GeometryUtility.GetSolids(element, doc).Solids;
                solidsOfElement.Add((element, solids));
            }
            double radius = 0;
            HashSet<Element> elementsAreCylinder = new HashSet<Element>();
            foreach (var pairs in solidsOfElement)
            {
                foreach (var solid in pairs.Item2)
                {
                    var tuple = SolidUtility.GetGroupedFacesFromSolid(doc, solid);

                    List<PlanarFace> planarFacesOfSolid = tuple.Item1;
                    List<CylindricalFace> cylindricalFaces = tuple.Item2;

                    if (planarFacesOfSolid.Count > 0 && cylindricalFaces.Count > 0)
                    {
                        // Lấy ra những mặt trụ song song với Z và mặt phẳng vuông góc với Z
                        var listcylindricalFace = cylindricalFaces.Where(c => Math.Abs(Math.Abs(c.Axis.Z) - 1) < CommonConstants.TOLERANCE).ToList();
                        var listPlanarFace = planarFacesOfSolid.Where(f => Math.Abs(Math.Abs(f.FaceNormal.Z) - 1) < CommonConstants.TOLERANCE).ToList();
                        if (listcylindricalFace.Count == 4 && listPlanarFace.Count == 2)
                        {
                            elementsAreCylinder.Add(pairs.Item1);
                            if (radius == 0)
                            {
                                radius = FaceUtility.GetRadius(listcylindricalFace.First());
                            }
                        }
                    }
                }
            }
            var (minPoint, maxPoint) = PointUtility.GetOverallBoundingBox(doc, collectedElements);

            if (minPoint != null && maxPoint != null)
            {
                minX = minPoint.X;
                maxX = maxPoint.X;
                minY = minPoint.Y;
                maxY = maxPoint.Y;
            }
            if (radius > 0)
            {
                solidsOfElement.RemoveAll(x => elementsAreCylinder.Contains(x.Item1));

                double gridSize = radius * 1.5;
                var squares = GeometryUtility.GenerateGridSquares(minX, maxX, minY, maxY, gridSize);

                var topElementIds = GetElementIdHighestOnEachSquare(doc, solidsOfElement, squares, maxPoint.Z, minPoint.Z);

                if (topElementIds.Count > 0)
                {
                    uidoc.Selection.SetElementIds(topElementIds);
                    uidoc.ShowElements(topElementIds);
                }
                else
                {
                    TaskDialog.Show("Error", "Does not exist any ElementId");
                }
            }
        }

        /// <summary>
        /// Hàm trả về những element cao nhất trong mỗi 1 square
        /// </summary>
        /// <param name="solidsOfElement"></param>
        /// <param name="squares"></param>
        /// <returns></returns>
        private List<ElementId> GetElementIdHighestOnEachSquare(Document doc, List<(Element, List<Solid>)> solidsOfElement, List<List<XYZ>> squares, double maxZ, double minZ)
        {
            HashSet<ElementId> topElementIds = new HashSet<ElementId>();
            foreach (var square in squares)
            {
                // Vẽ 1 đường line thẳng đứng dựa vào tâm hình vuông
                XYZ centerPoint = PointUtility.GetCenterPoint(square);
                XYZ maxPoint = PointUtility.SetPointWithNewZValue(centerPoint, maxZ);
                XYZ minPoint = PointUtility.SetPointWithNewZValue(centerPoint, minZ);

                Line line = Line.CreateBound(maxPoint, minPoint);

                List<(Element, List<Solid>)> solidsOfSquare = new List<(Element, List<Solid>)>();
                foreach (var pairs in solidsOfElement)
                {
                    foreach (var solid in pairs.Item2)
                    {
                        if (SolidUtility.IsLineIntersectSolid(line, solid))
                        {
                            solidsOfSquare.Add(pairs);
                            break;
                        }
                    }
                }
                if (solidsOfSquare.Count == 0) continue;

                double minDistance = double.MaxValue;
                Element highestElement = null;

                foreach (var pairs in solidsOfSquare)
                {
                    var (min, max) = PointUtility.GetBoundingBoxExtents(pairs.Item1);
                    if (max != null)
                    {
                        if (maxPoint.Z - max.Z < minDistance)
                        {
                            minDistance = maxPoint.Z - max.Z;
                            highestElement = pairs.Item1;
                        }
                    }
                }
                if (highestElement != null)
                {
                    topElementIds.Add(highestElement.Id);
                }
            }
            return topElementIds.ToList();
        }

        /// <summary>
        /// Kiểm tra xem element là pipe fiting hay duct fitting
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool CheckElementIsPipeFittingOrDuctFitting(Element element)
        {
            if (element is FamilyInstance fi)
            {
                var mepModel = fi.MEPModel;
                if (mepModel != null)
                {
                    if (fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting || fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Xử lý trường hợp element là railing
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        private void HandleRailingCase(Document doc, Railing railing)
        {
            double length = 0;
            List<Curve> curves = GetTopRailLines(doc, railing);
            foreach (Curve c in curves)
            {
                if (c is CylindricalHelix helix)
                {
                    length += helix.Length;
                }
                if (c is Arc arc)
                {
                    length += arc.Length;
                }
                if (c is Line line)
                {
                    if (!GeometryUtility.IsParallel(line.Direction, XYZ.BasisZ, CommonConstants.COSINE_ANGLE_TOLERANCE_1_DEGREE))
                    {
                        length += line.Length;
                    }
                }
            }
            TaskDialog.Show("Notif", "Length of railing is: " + length.ToString());
        }

        /// <summary>
        /// Hàm lấy ra danh sách curves của toprailing
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="railing"></param>
        /// <returns></returns>
        public List<Curve> GetTopRailLines(Document doc, Railing railing)
        {
            var result = new List<Curve>();

            ElementId topRailId = railing.TopRail;
            if (topRailId == ElementId.InvalidElementId) return result;

            TopRail topRail = doc.GetElement(topRailId) as TopRail;
            if (topRail == null) return result;

            IList<Curve> curves = topRail.GetPath();
            foreach (Curve c in curves)
            {
                //if (c is Line line)
                result.Add(c);
            }

            return result;
        }

        private void HandleInCaseHaveTwoCylinderInfos(Document doc, List<CylinderInfo> cylinderInfos)
        {
            if (cylinderInfos.Count == 2)
            {
                XYZ firstPoint = cylinderInfos[0].TopPoint;
                XYZ endPoint = cylinderInfos[1].TopPoint;
                double length = firstPoint.DistanceTo(endPoint);
                DrawPointLineArc.CreateModelLine(doc, firstPoint, endPoint, isRevitLink, transform, 1);
                TaskDialog.Show("Nofi", "The length of railing is : " + length.ToString());
            }
        }

        /// <summary>
        /// Hàm dùng để kiểm tra xem element bao gồm các khối solid liền nhau hay rời nhau
        /// </summary>
        /// <param name="areSolidsConnected"></param>
        /// <param name="cylinderInfos"></param>
        /// <param name="solids"></param>
        /// <returns></returns>
        private void CheckAreSolidsConnected(ref bool areSolidsConnected, List<CylinderInfo> cylinderInfos, List<Solid> solids)
        {
            for (int i = 0; i < cylinderInfos.Count - 1; i++)
            {
                List<XYZ> points = new List<XYZ>();
                XYZ p1 = cylinderInfos[i].TopPoint;
                XYZ p2 = cylinderInfos[i + 1].TopPoint;
                points.Add(p1);
                points.Add(p2);
                XYZ centerPoint = PointUtility.GetCenterPoint(points);
                // Tạo 1 line thẳng đứng từ tâm của 2 điểm, sau đó kiểm tra xem có cắt solid không
                Line line = Line.CreateUnbound(centerPoint, XYZ.BasisZ);
                foreach (Solid solid in solids)
                {
                    if (!SolidUtility.IsLineIntersectSolid(line, solid))
                    {
                        areSolidsConnected = false;
                        break;
                    }
                }
                if (!areSolidsConnected) break;
            }
        }

        private void PrepareDataForExecution(List<CylinderInfo> cylinderInfos, Document doc, UIDocument uiDoc, List<Mesh> meshes, List<Solid> solids)
        {
            if (meshes.Count == 0)
            {
                cylinderInfos = MergeCylinderInfos(cylinderInfos);
                if (cylinderInfos.Count == 2)
                {
                    HandleInCaseHaveTwoCylinderInfos(doc, cylinderInfos);
                }
            }
            //var mergedList = MergeCylinderInfos(cylinderInfos);
            //SortInstancesAlongLine(mergedList);

            if (meshes.Count > 0 || cylinderInfos.Count > 2)
            {
                //if (GroupPointsOnSamePlane(mergedList).Count == 0)
                //{
                //    var results = new List<List<CylinderInfo>> { mergedList };
                //    CalculateSpiralRailingLength(results, doc, uiDoc);
                //}
                //else
                //{
                SortInstancesAlongLine(cylinderInfos);
                GroupPointsForDistanceCalculation(cylinderInfos, meshes, doc);
                //GetLengthOfRailing(doc, result);
                //if (result.Count > 0)
                //{
                //    double length = 0;
                //    foreach (var group in result)
                //    {
                //        XYZ firstPoint = group.FirstOrDefault().TopPoint;
                //        XYZ endPoint = group.LastOrDefault().TopPoint;
                //        length += firstPoint.DistanceTo(endPoint);
                //        CreateModelLine(doc, firstPoint, endPoint);
                //    }
                //    TaskDialog.Show("Nofi", "The length of railing is : " + length.ToString());
                //}
                //}
            }
        }

        private void GetLengthOfRailing(Document doc, List<List<CylinderInfo>> result)
        {
            if (result.Count > 0)
            {
                double length = 0;
                foreach (var group in result)
                {
                    XYZ firstPoint = group.FirstOrDefault().TopPoint;
                    XYZ endPoint = group.LastOrDefault().TopPoint;
                    length += firstPoint.DistanceTo(endPoint);
                    DrawPointLineArc.CreateModelLine(doc, firstPoint, endPoint, isRevitLink, transform, 1);
                }
                //TaskDialog.Show("Nofi", "The length of railing is : " + length.ToString());
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
                c.TopPoint = PointUtility.SetPointWithNewZValue(c.TopPoint, minZ);
            }
        }

        private void GetTopViewShape(List<Mesh> meshes, List<CylinderInfo> cylinderInfos, Document doc)
        {
            List<XYZ> topPoints = new List<XYZ>();

            foreach (var ci in cylinderInfos)
            {
                topPoints.Add(ci.TopPoint);
            }

            CylinderInfo firstCylinderInfo = cylinderInfos.First();

            double radius = firstCylinderInfo.Radius;

            // Lấy ra tất cả các triangles của các meshes
            List<MeshTriangle> trianglesOfElement = new List<MeshTriangle>();
            HashSet<MeshTriangle> trianglesParallelToZ = new HashSet<MeshTriangle>();
            HashSet<MeshTriangle> trianglesPerpendicularToZ = new HashSet<MeshTriangle>();

            if (meshes.Count > 0)
            {
                foreach (var mesh in meshes)
                {
                    for (int i = 0; i < mesh.NumTriangles; i++)
                    {
                        trianglesOfElement.Add(mesh.get_Triangle(i));
                    }
                }
            }

            foreach (var tri in trianglesOfElement)
            {
                XYZ normal = TrianglesUtility.GetNormalFromTriangle(tri);
                if (FaceUtility.IsFacePerpendicularToAxis(normal, XYZ.BasisZ))
                {
                    trianglesPerpendicularToZ.Add(tri);
                }
                else if (FaceUtility.IsFaceParallelToZ(normal))
                {
                    trianglesParallelToZ.Add(tri);
                }
            }

            // Xóa những face thuộc trụ và những face thuộc hình tròn ở 2 đầu trụ
            trianglesOfElement.RemoveAll(t => trianglesParallelToZ.Contains(t));
            trianglesOfElement.RemoveAll(t => trianglesPerpendicularToZ.Contains(t));

            //TestFunction(trianglesOfElement, doc);
            //Hàm này trả về List<List<MeshTriangle>> phân chia thành các nhóm tam giác
            var topPointsAndTrianglesHighestOfEachPart = GetTrianglesHaveHigherZ(trianglesOfElement, radius, doc, topPoints);

            foreach (var (topPointsForEachPart, trianglesHighest) in topPointsAndTrianglesHighestOfEachPart)
            {
                //var trianglesHighest = topPointsAndTrianglesHighestOfEachPart[0];
                //var (trianglesHighest) = tupleValues.TrianglesHigher;
                //var topPointsForEachPart = tupleValues.TopPoints;

                if (topPointsForEachPart.Count == 1) continue;

                var cylinderInfosForEachPart = cylinderInfos
                    .Where(c => topPointsForEachPart.Any(p => c.TopPoint.IsAlmostEqualTo(p, CommonConstants.TOLERANCE)))
                    .ToList();

                SortInstancesAlongLine(cylinderInfosForEachPart);

                var result = GroupPointsOnSamePlane(cylinderInfosForEachPart);
                //if (result.Count > 0) continue;

                var remainingcylinderInfos = GetRemainCylinderInfo(cylinderInfosForEachPart, result);
                bool isComplex = false;
                if (remainingcylinderInfos.Count == 0)
                {
                    isComplex = IsElementComplex(doc, result);
                }

                //var pointsAsDiagonalLine = GroupPointsAsDiagonalLine(cylinderInfosForEachPart);

                //var remainPoins = cylinderInfosForEachPart.Where(a => !pointsAsDiagonalLine.Contains(a));

                //bool areAllCylindersMakeDiagonalLine = pointsAsDiagonalLine.Count == cylinderInfosForEachPart.Count;
                //bool areAllCylindersMakeStraightLine = GroupCylinderAsStraightLine(cylinderInfosForEachPart, null).Count == cylinderInfosForEachPart.Count;

                // Nếu các trụ thẳng và chỉ có 1 mặt phẳng
                //if (result.Count == 1 && remainingcylinderInfos.Count == 0
                //    && (areAllCylindersMakeStraightLine || areAllCylindersMakeDiagonalLine))
                //{
                //    CreateModelLine(doc, cylinderInfosForEachPart.First().TopPoint, cylinderInfosForEachPart.Last().TopPoint);
                //    continue;
                //}

                // Các trường hợp còn lại như thừa trụ lẻ, hoặc các trụ không thẳng
                if (isComplex == true || remainingcylinderInfos.Count > 0)
                {
                    HandleForMeshesComplex(trianglesHighest, topPointsForEachPart, doc, radius, result, remainingcylinderInfos);
                }
            }
        }

        /// <summary>
        /// Hàm xử lý những trường hợp element là mesh và có hình dạng phức tạp
        /// </summary>
        private void HandleForMeshesComplex(List<MeshTriangle> trianglesHighest, List<XYZ> topPointsForEachPart, Document doc, double radius, List<List<CylinderInfo>> result, List<CylinderInfo> remainingcylinderInfos)
        {
            List<Line> allLine = new List<Line>();

            HashSet<XYZ> topPointsViewed = new HashSet<XYZ>();
            XYZ firstPoint = null;
            XYZ lastPoint = null;
            List<MeshTriangle> trianglesHighestClone = trianglesHighest.ToList();
            List<XYZ> topPointsClone = topPointsForEachPart.ToList();
            while (topPointsForEachPart.Count > 1 && firstPoint == null)
            {
                GetLinesFromTriangles(trianglesHighest, topPointsForEachPart.FirstOrDefault(), radius, true, doc, topPointsForEachPart, topPointsViewed, ref firstPoint, null);

                topPointsForEachPart.RemoveAll(x => topPointsViewed.Any(y => x.IsAlmostEqualTo(y, CommonConstants.TOLERANCE)));
            }
            topPointsViewed.Clear();

            if (firstPoint != null)
            {
                int num = topPointsClone.Count;
                int indexToRemove = num;
                for (int i = 0; i < num; i++)
                {
                    if (topPointsClone[i].IsAlmostEqualTo(firstPoint, CommonConstants.TOLERANCE))
                    {
                        indexToRemove = i;
                        break;
                    }
                }
                if (indexToRemove != num)
                {
                    topPointsClone.RemoveAt(indexToRemove);
                }
                topPointsClone.Insert(0, firstPoint);
                if (topPointsClone.Count == num)
                {
                    while (topPointsClone.Count > 1 && firstPoint != null)
                    {
                        var lines = GetLinesFromTriangles(trianglesHighestClone, topPointsClone.First(), radius, true, doc, topPointsClone, topPointsViewed, ref lastPoint, null);

                        if (lines != null && lines.Count > 0)
                        {
                            allLine.AddRange(lines);
                        }
                        topPointsClone.RemoveAll(x => topPointsViewed.Any(y => x.IsAlmostEqualTo(y, CommonConstants.TOLERANCE)));
                    }
                }
            }

            if (firstPoint != null && lastPoint != null)
            {
                if (allLine.Count == 1 && result.Count == 1 && remainingcylinderInfos.Count == 0)
                {
                    DrawPointLineArc.CreateModelLine(doc, firstPoint, lastPoint, isRevitLink, transform, 1);
                }
                else if (allLine.Count >= 2)
                {
                    GetAllIntersecPoints(firstPoint, lastPoint, allLine, doc);
                }
            }
        }

        /// <summary>
        /// Loại bỏ những triangles cùng vị trí (x,y) nhưng thấp hơn
        /// </summary>
        /// <param name="triangles"></param>
        private List<(List<XYZ> TopPoints, List<MeshTriangle> TrianglesHigher)> GetTrianglesHaveHigherZ(List<MeshTriangle> triangles, double r, Document doc, List<XYZ> topPoints)
        {
            // Đoạn này dùng để gom nhóm các tam giác thuộc 1 grid
            //double gridSize = r * 1.75;
            //double gridSize = r * 4; //nhanh cho gần tất cả
            //double gridSize = r / 2;
            double gridSize = r * 2.25;
            var squares = GeometryUtility.GenerateGridSquares(minX, maxX, minY, maxY, gridSize);

            List<(XYZ, List<MeshTriangle>)> tupleValues = new List<(XYZ, List<MeshTriangle>)>();
            foreach (var square in squares)
            {
                List<MeshTriangle> triangleList = new List<MeshTriangle>();
                foreach (var tri in triangles)
                {
                    var vertices = TrianglesUtility.GetVerticesOfTriangle(tri);
                    // Nếu grid và tam giác giao nhau
                    if (GeometryUtility.AreTriangleAndSquareIntersecting(vertices, square))
                    {
                        triangleList.Add(tri);
                    }
                }
                if (triangleList.Count > 0)
                {
                    tupleValues.Add((PointUtility.GetCenterPoint(square), triangleList));
                }
            }
            // Gom nhóm các square nối với nhau thành 1 nhóm
            List<XYZ> centerPoints = new List<XYZ>();
            foreach (var tuple in tupleValues)
            {
                centerPoints.Add(tuple.Item1);
            }
            double gapOfNearPoints = gridSize * 1.5; // gần bằng căn bậc 2 của 2
            var groupPoints = PointUtility.GroupClosePoints(centerPoints, gapOfNearPoints);

            List<(List<XYZ> TopPoints, List<List<MeshTriangle>> GroupTriangles)> topPointsAndGroupTrianglesForEachPart =
                new List<(List<XYZ> TopPoints, List<List<MeshTriangle>> GroupTriangles)>();
            foreach (var group in groupPoints)
            {
                List<List<MeshTriangle>> groupTriangles = new List<List<MeshTriangle>>();
                foreach (XYZ p in group)
                {
                    foreach (var tuple in tupleValues)
                    {
                        if (p.IsAlmostEqualTo(tuple.Item1, CommonConstants.TOLERANCE))
                        {
                            groupTriangles.Add(tuple.Item2);
                            break;
                        }
                    }
                }

                //Chia topPoint theo từng phần của element
                List<XYZ> topPointsForEachPart = new List<XYZ>();
                foreach (XYZ p in topPoints)
                {
                    XYZ newPointOnXY = PointUtility.SetPointWithNewZValue(p, 0);
                    if (PointUtility.IsPointNearListPoint(newPointOnXY, group, gridSize))
                    {
                        topPointsForEachPart.Add(p);
                    }
                }

                topPointsAndGroupTrianglesForEachPart.Add((topPointsForEachPart, groupTriangles));
            }

            List<(List<XYZ> TopPoints, List<MeshTriangle> TrianglesHigher)> topPointsAndTrianglesHigherOfEachPart = new List<(List<XYZ> TopPoints, List<MeshTriangle> TrianglesHigher)>();
            foreach (var newTuple in topPointsAndGroupTrianglesForEachPart)
            {
                HashSet<MeshTriangle> meshTrianglesHigher = new HashSet<MeshTriangle>();

                foreach (var trianglesOnGrid in newTuple.GroupTriangles)
                {
                    double maxZ = double.MinValue;
                    XYZ maxPoint;

                    foreach (var tri in trianglesOnGrid)
                    {
                        double highest = TrianglesUtility.GetHighestAndLowestZPoint(tri).Max.Z;

                        if (highest > maxZ)
                        {
                            maxZ = highest;
                            maxPoint = TrianglesUtility.GetHighestAndLowestZPoint(tri).Max;
                        }
                    }

                    foreach (var tri1 in trianglesOnGrid)
                    {
                        if (meshTrianglesHigher.Contains(tri1)) continue;
                        double highest = TrianglesUtility.GetHighestAndLowestZPoint(tri1).Max.Z;
                        if (maxZ - highest < r)
                        {
                            meshTrianglesHigher.Add(tri1);
                        }
                    }
                }
                topPointsAndTrianglesHigherOfEachPart.Add((newTuple.TopPoints, meshTrianglesHigher.ToList()));
            }

            return topPointsAndTrianglesHigherOfEachPart;
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
        private List<(XYZ, XYZ)> GetAllIntersecPoints(XYZ firstPoint, XYZ lastPoint, List<Line> lines, Document doc)
        {
            List<(XYZ, XYZ)> pairs = new List<(XYZ, XYZ)>();
            List<XYZ> points = new List<XYZ>();
            //points.Add(firstPoint);

            LineUtility.MakeLinesCoplanar(lines);

            for (int i = 0; i < lines.Count - 1; i++)
            {
                XYZ intersectPoint = LineUtility.GetIntersectionPoint(lines[i], lines[i + 1]);
                if (intersectPoint != null)
                {
                    points.Add(intersectPoint);
                }
            }
            //Plane lastPlane = CreatePlaneParallelToZFromLine(lines.Last());
            //points.Add(GetProjectedPoint(lastPlane, lastPoint));
            points.Insert(0, LineUtility.GetPerpendicularProjectionPointOnLine(lines.First(), firstPoint));
            points.Add(LineUtility.GetPerpendicularProjectionPointOnLine(lines.Last(), lastPoint));
            for (int i = 0; i < points.Count - 1; i++)
            {
                pairs.Add((points[i], points[i + 1]));
            }

            // Chỉ có 2 đường
            //else
            //{
            //    Line firstLine = lines[0];
            //    Line lastLine = lines[1];
            //    // Nếu 2 đường không trùng nhau
            //    if (!AreLinesColinear(firstLine, lastLine))
            //    {
            //        XYZ intersectPoint = GetIntersectionPoint(firstLine, lastLine);
            //        if (intersectPoint != null)
            //        {
            //            pairs.Add((firstPoint, intersectPoint));
            //            pairs.Add((intersectPoint, lastPoint));
            //        }
            //    }
            //}
            //string str = "";
            //foreach (var a in pairs)
            //{
            //    str += a.ToString();
            //    str += ";";
            //}
            //TaskDialog.Show("adfd", str);
            foreach (var pair in pairs)
            {
                DrawPointLineArc.CreateModelLine(doc, pair.Item1, pair.Item2, isRevitLink, transform, 1);
            }

            return pairs;
        }

        private List<Line> GetLinesFromTriangles(List<MeshTriangle> triangles,
            XYZ point, double radius, bool isTopPointOfCylinder, Document doc, List<XYZ> topPoints, HashSet<XYZ> topPointsViewed, ref XYZ firstPoint, XYZ originPointToCreateLine)
        {
            List<Line> lines = new List<Line>();
            (XYZ, Line) pointAndLine;

            var pointAndAxis = GetPointAndAxisOfTriangles(point, triangles, radius, isTopPointOfCylinder, topPoints);

            if (pointAndAxis.Axis == null)
            {
                XYZ p = GetTopPoint(topPoints, point, radius, originPointToCreateLine, false);
                if (p != null)
                {
                    topPointsViewed.Add(p);
                    // Trường hợp point truyền vào lúc đầu là ReferencePoint mà axis trả về null
                    // và tồn tại điểm p thì đó là last point
                    if (!isTopPointOfCylinder)
                    {
                        firstPoint = p;
                    }
                }
                return null;
            }
            Line line = LineUtility.CreateLine(pointAndAxis.OriginPoint, pointAndAxis.Axis, lengthOfLine);
            lines.Add(line);

            pointAndLine = (pointAndAxis.OriginPoint, line);

            //Thêm đoạn này xem có lỗi gì không
            // Nếu toppoint được truyền vào hàm thì sẽ cho vào toppoitviewed
            if (isTopPointOfCylinder)
            {
                topPointsViewed.Add(point);
            }

            XYZ referencePoint = GetRemainTriangles(pointAndLine, triangles, radius, doc);

            if (referencePoint != null)
            {
                XYZ p = GetTopPoint(topPoints, referencePoint, radius, pointAndLine.Item1, true);
                if (p != null)
                {
                    topPointsViewed.Add(p);
                }
                var subLines = GetLinesFromTriangles(triangles, referencePoint, radius, false, doc, topPoints, topPointsViewed, ref firstPoint, pointAndLine.Item1);
                if (subLines != null && subLines.Count > 0)
                {
                    lines.AddRange(subLines);
                }
            }

            return lines;
        }

        /// <summary>
        /// Hàm kiểm tra 1 point truyền vào có nằm trong listpoint không
        /// </summary>
        /// <param name="topPoints"></param>
        /// <param name="point"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        private XYZ GetTopPoint(List<XYZ> topPoints, XYZ point, double radius, XYZ originPointToCreateLine, bool hasAxis)
        {
            List<XYZ> list = new List<XYZ>();
            List<double> checkDistances = new List<double> { radius * 6, radius * 12 };
            for (int i = 0; i < topPoints.Count; i++)
            {
                if (point.IsAlmostEqualTo(topPoints[i], CommonConstants.TOLERANCE))
                {
                    return point;
                }
                //else if (point.DistanceTo(topPoints[i]) < radius * 6)
                else if (checkDistances.Any(l => point.DistanceTo(topPoints[i]) < l))
                {
                    list.Add(topPoints[i]);
                }
                //else if (point.DistanceTo(topPoints[i]) < radius * 12)
                //{
                //    list.Add(topPoints[i]);
                //}
            }
            if (list.Count == 1)
            {
                return list[0];
            }
            else if (list.Count == 2 && originPointToCreateLine != null)
            {
                double dis1 = originPointToCreateLine.DistanceTo(list[0]);
                double dis2 = originPointToCreateLine.DistanceTo(list[1]);

                bool takeFirst = (dis1 > dis2) != hasAxis;
                return takeFirst ? list[0] : list[1];
            }

            return null;
        }

        /// <summary>
        /// Hàm xóa đi những tam giác thuộc line và trả về danh sách các điểm làm mốc để xét đối với những line khác
        /// </summary>
        /// <param name="pointAndLines"></param>
        /// <param name="triangles"></param>
        /// <param name="topZ"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private XYZ GetRemainTriangles((XYZ, Line) tuple, List<MeshTriangle> triangles, double r, Document doc)
        {
            double distance = r * 4;
            List<MeshTriangle> trianglesNearLine = new List<MeshTriangle>();
            foreach (var triangle in triangles)
            {
                XYZ point1 = triangle.get_Vertex(0);

                // Kiểm tra những point trong tam giác nào thỏa mãn nằm cách đường line 1 khoảng thì gom vào 1 nhóm
                //if (DistancePointToLine(point1, tuple.Item2) < (topZ - tuple.Item1.Z) + 0.04)
                if (LineUtility.DistancePointToLine(point1, tuple.Item2) < distance)
                {
                    trianglesNearLine.Add(triangle);
                }
            }
            // Danh sách tam giác song song với line
            List<MeshTriangle> trianglesParallelToLine = TrianglesUtility.GetTrianglesParallelToLine(tuple.Item2, trianglesNearLine);
            XYZ farthestPoint = GetFarthestPoint(tuple.Item1, trianglesParallelToLine);
            //targetPoint.Add(farthestPoint);
            HashSet<MeshTriangle> trianglesViewed = new HashSet<MeshTriangle>(trianglesParallelToLine);
            //trianglesViewed.AddRange(trianglesParallelToLine);

            //List<MeshTriangle> remainTriangles = GetElementsInANotInB(triangles, trianglesViewed);
            triangles.RemoveAll(x => trianglesViewed.Contains(x));
            return farthestPoint;
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
            List<XYZ> vertexs = GetVerticesOfAllTriangles(triangles).Vertexs;
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
            //List<double> checkDistances = new List<double> { radius * 3, radius * 2.5, radius * 2, radius * 1.5, radius };
            List<double> checkDistances = new List<double> { radius * 3 };

            foreach (var lengthOfEdge in checkDistances)
            {
                foreach (var tr in triangles)
                {
                    var vertices = TrianglesUtility.GetVerticesOfTriangle(tr);
                    if (vertices.Any(pt => pt.DistanceTo(point) < length + radius * 2))
                    {
                        var edges = TrianglesUtility.GetTriangleEdges(tr);
                        if (edges.Any(edge => edge.Length > lengthOfEdge))
                        {
                            sublist.Add(tr);
                        }
                    }

                    //
                    //foreach (var pt in vertices)
                    //{
                    //    double dist = pt.DistanceTo(point);
                    //    if (dist < length + radius * 2)
                    //    {
                    //        int count = 0;
                    //        List<Line> edges = GetTriangleEdges(tr);
                    //        foreach (Line line in edges)
                    //        {
                    //            if (line.Length > radius * 3)
                    //            {
                    //                count++;
                    //                break;
                    //            }
                    //        }
                    //        if (count > 0)
                    //        {
                    //            sublist.Add(tr);
                    //            break;
                    //        }
                    //    }
                    //}
                }
                if (sublist.Count > 1)
                    break;
            }
            return sublist;
        }

        /// <summary>
        /// Hàm lấy ra danh sách các điểm của tất cả các triangles
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        private (List<XYZ> Vertexs, Dictionary<XYZ, List<MeshTriangle>> VertexToTriangles) GetVerticesOfAllTriangles(List<MeshTriangle> triangles)
        {
            HashSet<XYZ> vertexs = new HashSet<XYZ>();

            /// Vertex thuộc những tam giác nào
            Dictionary<XYZ, List<MeshTriangle>> vertexToTriangles = new Dictionary<XYZ, List<MeshTriangle>>(new XYZComparer(CommonConstants.TOLERANCE));

            foreach (var tri in triangles)
            {
                List<XYZ> vertexsOfEachTriangle = TrianglesUtility.GetVerticesOfTriangle(tri);
                foreach (XYZ vertex in vertexsOfEachTriangle)
                {
                    if (!vertexToTriangles.TryGetValue(vertex, out var list))
                    {
                        list = new List<MeshTriangle>();
                        vertexToTriangles[vertex] = list;
                    }
                    list.Add(tri);
                }

                vertexs.UnionWith(vertexsOfEachTriangle);
            }
            return (vertexs.ToList(), vertexToTriangles);
        }

        /// <summary>
        /// Hàm trả về 1 điểm origin và vector chỉ phương để vẽ line
        /// </summary>
        /// <param name="A"></param>
        /// <param name="triangles"></param>
        /// <param name="r"></param>
        /// <param name="topZ"></param>
        /// <param name="IsOrigin"></param>
        /// <returns></returns>
        private (XYZ OriginPoint, XYZ Axis) GetPointAndAxisOfTriangles(XYZ A, List<MeshTriangle> triangles, double r, bool IsOrigin, List<XYZ> topPoints)
        {
            var tupleValues = GetVerticesOfAllTriangles(triangles);
            List<XYZ> vertexs = tupleValues.Vertexs;
            var vertexToTriangles = tupleValues.VertexToTriangles;
            // Tìm điểm xa nhất với điểm A trong bán kính cho trước
            // B nên thuộc tam giác có cạnh dài hơn 3r
            XYZ B = null;

            double maxDisToA = double.MinValue;
            //double minLengthToGetB = r * 6;
            //double maxLengthToGetB = r * 14;
            //double maxLengthToGetB = r * 12;
            //List<double> checkDistances = new List<double> { minLengthToGetB, maxLengthToGetB };
            List<double> checkDistances = new List<double> { r * 6, r * 14 };
            List<double> lengthOfEdge = new List<double> { r * 3 };
            //double minLengthOfEdge = r * 3;

            foreach (var lengthToGetB in checkDistances)
            {
                foreach (var l in lengthOfEdge)
                {
                    foreach (var v in vertexs)
                    {
                        double distToA = A.DistanceTo(v);
                        if (distToA > lengthToGetB)
                            continue;

                        if (vertexToTriangles.TryGetValue(v, out var triList))
                        {
                            foreach (var tri in triList)
                            {
                                if (TrianglesUtility.HasEdgeLongerThan(tri, l))
                                {
                                    if (distToA > maxDisToA)
                                    {
                                        maxDisToA = distToA;
                                        B = v;
                                    }

                                    break; // đã xác định v nằm trên tam giác hợp lệ -> không cần kiểm tra thêm tam giác
                                }
                            }
                        }
                    }
                    if (B != null)
                        break;
                }
                // Nếu đã tìm được B ở khoảng cách hiện tại thì dừng luôn
                if (B != null)
                    break;
            }

            //Nếu B null thì trụ nằm giữa của 1 thanh
            if (B == null)
            {
                return (null, null);
            }
            // Khoảng cách từ A đến B cộng thêm 1 khoảng 2r

            double length = A.DistanceTo(B);

            List<MeshTriangle> subListTriangleNearB = GetSublistTriangles(triangles, A, length, r);
            if (subListTriangleNearB.Count > 1)
            {
                XYZ axis = GetAxisFromTriangles(subListTriangleNearB);
                return (B, axis);
                //if (IsOrigin)
                //{
                //    return (A, axis);
                //}
                //else
                //{
                //    return (B, axis);
                //}
            }
            else
            {
                return (null, null);
            }
        }

        /// <summary>
        /// Tìm tia song song với trục của trụ bằng cách lấy ra 2 tam giác bất kỳ có normal không
        /// song song với nhau, sau đó cùng crossproduct 2 normal đó
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        private XYZ GetAxisFromTriangles(List<MeshTriangle> triangles)
        {
            MeshTriangle firstTriangle = triangles.FirstOrDefault();
            XYZ firstNormal = TrianglesUtility.GetNormalFromTriangle(firstTriangle);
            XYZ secondNormal = null;
            MeshTriangle secondTriangle = null;
            for (int i = 1; i < triangles.Count; i++)
            {
                XYZ normal = TrianglesUtility.GetNormalFromTriangle(triangles[i]);
                // Nếu 2 vector không song song với nhau
                if (!GeometryUtility.IsParallel(firstNormal, normal, CommonConstants.COSINE_ANGLE_TOLERANCE_5_DEGREE))
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
            if (GeometryUtility.IsParallel(axisVector, XYZ.BasisZ, CommonConstants.COSINE_ANGLE_TOLERANCE_5_DEGREE))
            {
                return PointUtility.SetPointWithNewZValue(axisVector, 0);
            }

            return axisVector;
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
                        cylinderInfo.TopPoint = PointUtility.SetPointWithNewZValue(cylinderInfo.TopPoint, maxTop);
                        cylinderInfo.BottomPoint = PointUtility.SetPointWithNewZValue(cylinderInfo.BottomPoint, minBottom);
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
                    if (Math.Abs(current.TopPoint.X - other.TopPoint.X) < CommonConstants.TOLERANCE && Math.Abs(current.TopPoint.Y - other.TopPoint.Y) < CommonConstants.TOLERANCE)
                    {
                        CollectConnected(other, input, group, visited);
                    }
                }
            }
        }

        /// <summary>
        /// Lấy ra danh sách các CylinderInfo được tạo ra từ các CylindricalFace
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private (List<Mesh> Meshs, List<CylinderInfo> CylinderInfos, List<Solid> Solids) GetCylinderInfosFromElements(Element element, Document doc)
        {
            List<CylinderInfo> cylinderInfos = new List<CylinderInfo>();

            var tupleValue = GeometryUtility.GetSolids(element, doc);
            List<Solid> solids = tupleValue.Solids;

            List<Mesh> meshes = tupleValue.Meshes;

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
                    var (planarFacesOfSolid, cylindricalFaces) = SolidUtility.GetGroupedFacesFromSolid(doc, solid);
                    //List<PlanarFace> planarFacesOfSolid = tuple.Item1;
                    //List<CylindricalFace> cylindricalFaces = tuple.Item2;

                    if (planarFacesOfSolid.Count > 0 && cylindricalFaces.Count > 0)
                    {
                        // Lấy ra tất cả các mesh từ các face

                        //AddMesh(meshes, planarFacesOfSolid);
                        //AddMesh(meshes, cylindricalFaces);

                        // Lấy ra những mặt trụ song song với Z và mặt phẳng vuông góc với Z
                        var listcylindricalFace = cylindricalFaces.Where(c => Math.Abs(Math.Abs(c.Axis.Z) - 1) < CommonConstants.TOLERANCE).ToList();
                        var listPlanarFace = planarFacesOfSolid.Where(f => Math.Abs(Math.Abs(f.FaceNormal.Z) - 1) < CommonConstants.TOLERANCE).ToList();
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
                                    if (!listcylindricalFace[i].Origin.IsAlmostEqualTo(firstOrigin, CommonConstants.TOLERANCE))
                                    {
                                        newListFace.Add(listcylindricalFace[i]);
                                        firstOrigin = listcylindricalFace[i].Origin;
                                    }
                                }
                                foreach (var face in newListFace)
                                {
                                    var (maxPoint, minPoint) = GetHighestAndLowestPointOfFace(face);
                                    if (maxPoint != null && minPoint != null)
                                    {
                                        double maxZ = maxPoint.Z;
                                        double minZ = minPoint.Z;

                                        cylinderInfos.Add(CreateCylinderInfo(face, maxZ, minZ));
                                    }
                                }
                            }
                        }
                    }
                    else if (cylindricalFaces.Count == 0 && planarFacesOfSolid.Count > 0)
                    {
                        planarFacesOfElement.AddRange(planarFacesOfSolid);
                    }
                }

                if (planarFacesOfElement.Count > 0)
                {
                    GeometryUtility.AddMesh(meshes, planarFacesOfElement);
                    foreach (PlanarFace p in planarFacesOfElement)
                    {
                        //Mesh mesh = p.Triangulate();
                        //meshes.Add(mesh);
                        // Kiểm tra xem facenormal của planarface có gần vuông góc với Z không
                        if (FaceUtility.IsFaceParallelToZ(p.FaceNormal))
                        {
                            Mesh mesh = p.Triangulate();
                            AddMeshTriangles(triangles, mesh);
                        }
                    }
                }
            }
            //var result = GroupFacesBySharedVertices(planarFacesParallelToZ);
            var result = TrianglesUtility.GroupMeshTrianglesBySharedVertices(triangles);
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
            return (meshes, cylinderInfos, solids);
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
                XYZ normal = TrianglesUtility.GetNormalFromTriangle(triangle);
                if (FaceUtility.IsFaceParallelToZ(normal))
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
                var tuple = TrianglesUtility.GetHighestAndLowestZPoint(triangle);
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

                points.AddRange(TrianglesUtility.GetVerticesOfTriangle(triangle));
            }
            XYZ centerPoint = PointUtility.GetCenterPoint(points);

            cylinderInfo.TopPoint = PointUtility.SetPointWithNewZValue(centerPoint, highestPoint.Z);
            cylinderInfo.BottomPoint = PointUtility.SetPointWithNewZValue(centerPoint, lowestPoint.Z);
            cylinderInfo.Radius = highestPoint.DistanceTo(cylinderInfo.TopPoint);
            //if (defaultRadius < cylinderInfo.Radius)
            //{
            //    defaultRadius = cylinderInfo.Radius;
            //}

            return cylinderInfo;
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

            cylinderInfo.Radius = FaceUtility.GetRadius(face);
            cylinderInfo.TopPoint = PointUtility.SetPointWithNewZValue(face.Origin, maxZ);
            cylinderInfo.BottomPoint = PointUtility.SetPointWithNewZValue(face.Origin, minZ);

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
                DrawPointLineArc.CreateModelArcFrom3Points(uiDoc, doc, group[i - 2].TopPoint, group[i].TopPoint, group[i - 1].TopPoint, isRevitLink, transform, 1);
            }
            if (num % 2 == 0)
            {
                length += group[num - 1].TopPoint.DistanceTo(group[num - 2].TopPoint);
                DrawPointLineArc.CreateModelArcFrom3Points(uiDoc, doc, group[num - 3].TopPoint, group[num - 1].TopPoint, group[num - 2].TopPoint, isRevitLink, transform, 1);
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
                        //if (dot > COSINE_ANGLE_TOLERANCE_1_DEGREE)
                        if (dot > CommonConstants.COSINE_ANGLE_TOLERANCE_1_DEGREE / 2)
                            continue;

                        // Tạo mặt phẳng
                        Plane plane = Plane.CreateByNormalAndOrigin(normal, p1);

                        // Gom các điểm thuộc mặt phẳng này
                        List<CylinderInfo> group = new List<CylinderInfo>();
                        foreach (var pt in cylinderInfos)
                        {
                            if (PlaneUtility.IsPointOnPlane(plane, pt.TopPoint))
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
                    }
                }
            }

            return result;
        }

        private bool ValidateInstanceSpacing(List<CylinderInfo> pointsAsDiagonalLine, CylinderInfo remainCylinderInfo)
        {
            XYZ remainPoint = PointUtility.SetPointWithNewZValue(remainCylinderInfo.TopPoint, 0);

            XYZ firstPoint = PointUtility.SetPointWithNewZValue(pointsAsDiagonalLine[0].TopPoint, 0);
            XYZ nextPoint = PointUtility.SetPointWithNewZValue(pointsAsDiagonalLine[1].TopPoint, 0);
            double distance = firstPoint.DistanceTo(nextPoint);

            // Khoảng cách từ điểm thừa đến điểm gần nhất trong danh sách
            double minDistance = pointsAsDiagonalLine.Min(p => PointUtility.SetPointWithNewZValue(p.TopPoint, 0).DistanceTo(remainPoint));
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
            List<CylinderInfo> cylinderInfosViewed = new List<CylinderInfo>();
            //CylinderInfo remainCylinderInfo = remainCylinderInfos.FirstOrDefault();
            CylinderInfo firstCylinderInfo = pointsAsStraightLine.FirstOrDefault();

            foreach (var remainCylinderInfo in remainCylinderInfos)
            {
                if (remainCylinderInfo.TopPoint.Z < firstCylinderInfo.TopPoint.Z
                    && remainCylinderInfo.BottomPoint.Z > firstCylinderInfo.BottomPoint.Z)
                {
                    remainCylinderInfo.TopPoint = PointUtility.SetPointWithNewZValue(remainCylinderInfo.TopPoint, firstCylinderInfo.TopPoint.Z);
                    pointsAsStraightLine.Add(remainCylinderInfo);
                    SortInstancesAlongLine(pointsAsStraightLine);
                    cylinderInfosViewed.Add(remainCylinderInfo);
                }
            }

            remainCylinderInfos.RemoveAll(c => cylinderInfosViewed.Contains(c));
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
                if (GeometryUtility.CompareDouble(C.Z, e.TopPoint.Z) && (!GeometryUtility.CompareDouble(C.X, e.TopPoint.X) || !GeometryUtility.CompareDouble(C.Y, e.TopPoint.Y)))
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
        private void HandleDiagonalAndStraightLines(List<CylinderInfo> pointsAsDiagonalLine, List<CylinderInfo> pointsAsStraightLine)
        {
            // kiểm tra có điểm chung không
            bool haveCommonItem = false;
            foreach (var t in pointsAsStraightLine)
            {
                foreach (var c in pointsAsDiagonalLine)
                {
                    if (Math.Abs(t.TopPoint.X - c.TopPoint.X) < CommonConstants.TOLERANCE
                        && Math.Abs(t.TopPoint.Y - c.TopPoint.Y) < CommonConstants.TOLERANCE)
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

                    XYZ E = PlaneUtility.GetProjectedPoint(groupPlane, newA);
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
                        if (PlaneUtility.IsPointOnPlane(aePlane, info.TopPoint))
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
        /// Hàm lấy ra những cylinderInfo thừa không tạo thành 1 mặt phẳng với các cylinderInfo khác
        /// </summary>
        /// <param name="cylinderInfos"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private List<CylinderInfo> GetRemainCylinderInfo(List<CylinderInfo> cylinderInfos, List<List<CylinderInfo>> result)
        {
            HashSet<CylinderInfo> allGroupedPoints = new HashSet<CylinderInfo>(result.SelectMany(g => g));
            var remainingcylinderInfos = cylinderInfos.Where(p => !allGroupedPoints.Any(q => q.Equals(p))).ToList();
            return remainingcylinderInfos;
        }

        private bool IsElementComplex(Document doc, List<List<CylinderInfo>> result)
        {
            List<List<CylinderInfo>> newResult = new List<List<CylinderInfo>>();
            foreach (var group in result)
            {
                SortInstancesAlongLine(group);
                var newGroup = CloneList(group);

                // Danh sách chéo
                List<CylinderInfo> pointsAsDiagonalLine = GroupPointsAsDiagonalLine(newGroup);
                // Danh sách thẳng
                List<CylinderInfo> pointsAsStraightLine = GroupCylinderAsStraightLine(newGroup, pointsAsDiagonalLine);

                HashSet<CylinderInfo> excluded = new HashSet<CylinderInfo>(pointsAsStraightLine.Concat(pointsAsDiagonalLine));

                List<CylinderInfo> remainCylinderInfos = newGroup.Where(x => !excluded.Contains(x)).ToList();

                // Xử lý trường hợp có điểm thừa không thuộc danh sách thẳng
                if (remainCylinderInfos.Count > 0 && pointsAsStraightLine.Count > 0)
                {
                    HandleSinglePointOutOfStraightLine(remainCylinderInfos, pointsAsStraightLine);
                }

                // Trường hợp này nếu là solid thì nên chuyển tất cả solid về mesh rồi tính lại từ đầu theo mesh
                // Còn trường hợp là mesh thì sẽ đưa về tính theo cách tổng quát
                if (remainCylinderInfos.Count > 0)
                {
                    return true;
                    //IsSolidComplex = true;
                    //IsMeshComplex = true;
                    //break;
                }

                // Xử lý trường hợp có 1 điểm thừa không thuộc danh sách chéo
                //if (remainCylinderInfos.Count == 1 && pointsAsDiagonalLine.Count > 0) // có tồn tại 1 điểm thừa
                //{
                //    HandleSinglePointOutOfDiagonalLine(newResult, remainCylinderInfos, pointsAsDiagonalLine);
                //}

                // Trường hợp không tồn tại điểm thừa
                // Tồn tại cả danh sách thẳng và chéo
                if (pointsAsStraightLine.Count > 0 && pointsAsDiagonalLine.Count > 0)
                {
                    HandleDiagonalAndStraightLines(pointsAsDiagonalLine, pointsAsStraightLine);
                }
                if (pointsAsStraightLine.Count > 0)
                {
                    newResult.Add(pointsAsStraightLine);
                }
                if (pointsAsDiagonalLine.Count > 0)
                {
                    newResult.Add(pointsAsDiagonalLine);
                }
            }
            GetLengthOfRailing(doc, newResult);
            return false;
        }

        /// <summary>
        /// Nhóm các hình trụ có thành các nhóm nhỏ để tính khoảng cách, các nhóm nhỏ bao gồm các nhóm chéo, nhóm thẳng
        /// </summary>
        /// <param name="cylinderInfos"></param>
        /// <returns></returns>
        private void GroupPointsForDistanceCalculation(List<CylinderInfo> cylinderInfos, List<Mesh> meshes, Document doc)
        {
            var result = GroupPointsOnSamePlane(cylinderInfos);

            var remainingcylinderInfos = GetRemainCylinderInfo(cylinderInfos, result);

            //List<List<CylinderInfo>> newResult = new List<List<CylinderInfo>>();

            bool hasMeshes = meshes.Count > 0;
            bool hasRemaining = remainingcylinderInfos.Count > 0;
            bool isMultipleResults = result.Count > 1;

            // Trường hợp mesh có thừa 1 điểm hoặc có nhiều mặt phẳng
            if (hasMeshes && (hasRemaining || isMultipleResults))
            {
                GetTopViewShape(meshes, cylinderInfos, doc);
            }
            // Trường hợp solid hoặc mesh chỉ có 1 mặt phẳng (không có điểm thừa)
            else
            {
                bool IsMeshComplex = false;
                bool IsSolidComplex = false;
                if (IsElementComplex(doc, result))
                {
                    if (hasMeshes)
                    {
                        IsMeshComplex = true;
                    }
                    else
                    {
                        IsSolidComplex = true;
                    }
                }
                //bool IsMeshComplex = false;
                //bool IsSolidComplex = false;

                // 1 group là 1 mặt phẳng, 1 mặt phẳng yêu cầu có ít nhất 3 trụ
                //foreach (var group in result)
                //{
                //    SortInstancesAlongLine(group);
                //    var newGroup = CloneList(group);

                //    // Danh sách chéo
                //    List<CylinderInfo> pointsAsDiagonalLine = GroupPointsAsDiagonalLine(newGroup);
                //    // Danh sách thẳng
                //    List<CylinderInfo> pointsAsStraightLine = GroupCylinderAsStraightLine(newGroup, pointsAsDiagonalLine);

                //    HashSet<CylinderInfo> excluded = new HashSet<CylinderInfo>(pointsAsStraightLine.Concat(pointsAsDiagonalLine));

                //    List<CylinderInfo> remainCylinderInfos = newGroup.Where(x => !excluded.Contains(x)).ToList();

                //    // Xử lý trường hợp có điểm thừa không thuộc danh sách thẳng
                //    if (remainCylinderInfos.Count > 0 && pointsAsStraightLine.Count > 0)
                //    {
                //        HandleSinglePointOutOfStraightLine(remainCylinderInfos, pointsAsStraightLine);
                //    }

                //    // Trường hợp này nếu là solid thì nên chuyển tất cả solid về mesh rồi tính lại từ đầu theo mesh
                //    // Còn trường hợp là mesh thì sẽ đưa về tính theo cách tổng quát
                //    if (remainCylinderInfos.Count > 0)
                //    {
                //        IsSolidComplex = true;
                //        IsMeshComplex = true;
                //        break;
                //    }

                //    // Xử lý trường hợp có 1 điểm thừa không thuộc danh sách chéo
                //    //if (remainCylinderInfos.Count == 1 && pointsAsDiagonalLine.Count > 0) // có tồn tại 1 điểm thừa
                //    //{
                //    //    HandleSinglePointOutOfDiagonalLine(newResult, remainCylinderInfos, pointsAsDiagonalLine);
                //    //}

                //    // Trường hợp không tồn tại điểm thừa
                //    // Tồn tại cả danh sách thẳng và chéo
                //    if (pointsAsStraightLine.Count > 0 && pointsAsDiagonalLine.Count > 0)
                //    {
                //        HandleDiagonalAndStraightLines(pointsAsDiagonalLine, pointsAsStraightLine);
                //    }

                //    if (pointsAsStraightLine.Count > 0)
                //    {
                //        newResult.Add(pointsAsStraightLine);
                //    }
                //    if (pointsAsDiagonalLine.Count > 0)
                //    {
                //        newResult.Add(pointsAsDiagonalLine);
                //    }
                //    //if (!newResult.Contains(pointsAsDiagonalLine) && pointsAsDiagonalLine.Count > 0)
                //    //{
                //    //    newResult.Add(pointsAsDiagonalLine);
                //    //}
                //}

                // Trường hợp mesh chỉ có 1 mặt phẳng nhưng thứ tự các trụ phức tạp, không thẳng hết hoặc chéo hết
                if (IsMeshComplex)
                {
                    GetTopViewShape(meshes, cylinderInfos, doc);
                }

                // Trường hợp solid phức tạp, có những điểm thừa không thuộc nhóm các trụ thẳng hoặc chéo
                //if (IsSolidComplex)
                //{
                //    //var meshes = ChangeSolidtoMesh(solids);
                //    //GetTopViewShape(meshes, cylinderInfos, doc);
                //}

                // Trường hợp thừa 1-2 điểm không tạo thành 1 mặt phẳng
                //HashSet<CylinderInfo> allGroupedPoints = new HashSet<CylinderInfo>(result.SelectMany(g => g));
                //var remainingcylinderInfos = cylinderInfos.Where(p => !allGroupedPoints.Any(q => q.Equals(p))).ToList();
                //List<CylinderInfo> usedCylinderInfo = new List<CylinderInfo>();
                //if (remainingcylinderInfos.Count > 0)
                //{
                //    HandleRedundantPoint(newResult, remainingcylinderInfos, usedCylinderInfo);
                //}
            }
            //return newResult;
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

            if (Math.Abs(dZ) < CommonConstants.TOLERANCE)
            {
                return Math.Abs(zE - B.Z) < CommonConstants.TOLERANCE ? E : null;
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
            XYZ C = PointUtility.SetPointWithNewZValue(A, 0);
            Plane plane = Plane.CreateByThreePoints(A, B, C);
            XYZ newEOnPlane = PlaneUtility.GetProjectedPoint(plane, E);

            double t;

            if (Math.Abs(B.X - A.X) > CommonConstants.TOLERANCE)
            {
                t = (newEOnPlane.X - A.X) / (B.X - A.X);
            }
            else if (Math.Abs(B.Y - A.Y) > CommonConstants.TOLERANCE)
            {
                t = (newEOnPlane.Y - A.Y) / (B.Y - A.Y);
            }
            else
            {
                // Đường AB thẳng đứng (X và Y không đổi)
                // Kiểm tra xem E có cùng X, Y không
                if (Math.Abs(newEOnPlane.X - A.X) < CommonConstants.TOLERANCE && Math.Abs(newEOnPlane.Y - A.Y) < CommonConstants.TOLERANCE)
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
        /// Nhóm các trụ có toppoint bằng nhau hoặc bottom point bằng nhau
        /// </summary>
        /// <param name="group"></param>
        /// <param name="pointsAsDiagonalLine"></param>
        /// <returns></returns>
        private List<CylinderInfo> GroupCylinderAsStraightLine(List<CylinderInfo> group, List<CylinderInfo> pointsAsDiagonalLine)
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
            if (subList.Count > 2)
            {
                result.AddRange(subList);
                // Kiểm tra xem hàng chéo có chung trụ với hàng thẳng không, để khi thay đổi chiều cao hàng thẳng thì không ảnh hưởng đến hàng chéo
                if (pointsAsDiagonalLine != null && pointsAsDiagonalLine.Count > 0)
                {
                    ChangeGroupPointsAsDiagonalLine(pointsAsDiagonalLine, result);
                }

                double topZ = result.Max(c => c.TopPoint.Z);
                foreach (var c in result)
                {
                    c.TopPoint = PointUtility.SetPointWithNewZValue(c.TopPoint, topZ);
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
                if (!GeometryUtility.IsParallel(AB, AC, CommonConstants.COSINE_ANGLE_TOLERANCE_5_DEGREE))
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
    }

    /// <summary>
    /// Lớp dùng để lưu thông tin cần thiết của 1 cột trụ như bán kính, điểm cao nhất và thấp nhất
    /// </summary>
    public class CylinderInfo
    {
        public double Radius { get; set; }
        public XYZ TopPoint { get; set; }
        public XYZ BottomPoint { get; set; }

        //public CylinderInfo(double r, XYZ top,XYZ bottom)
        //{
        //    this.Radius = r;
        //    this.TopPoint = top;
        //    this.BottomPoint = bottom;
        //}

        public CylinderInfo Clone()
        {
            return new CylinderInfo { Radius = this.Radius, TopPoint = this.TopPoint, BottomPoint = this.BottomPoint };
            //return new CylinderInfo (Radius,TopPoint,BottomPoint);
        }
    }
}