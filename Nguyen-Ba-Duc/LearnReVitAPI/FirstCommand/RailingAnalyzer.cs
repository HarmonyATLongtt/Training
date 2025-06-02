using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using OpenQA.Selenium.BiDi.Modules.Session;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class RailingAnalyzer : IExternalCommand
    {
        private double tolerance = 1e-6;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                IList<Reference> selectedRefs = uidoc.Selection.PickObjects(ObjectType.Element, "Chọn các đối tượng");

                if (selectedRefs.Count == 0)
                {
                    TaskDialog.Show("Thông báo", "Không có đối tượng nào được chọn.");
                    return Result.Cancelled;
                }
                List<CylinderInfo> cylinderInfosOfElements = new List<CylinderInfo>();

                foreach (Reference r in selectedRefs)
                {
                    ElementId elementId = r.ElementId;
                    Element element = doc.GetElement(elementId);
                    List<CylinderInfo> cylinderInfos = GetCylinderInfosFromElements(element, doc);

                    // Trường hợp model là 1 khối thống nhất
                    if (cylinderInfos.Count > 1)
                    {
                        PrepareDataForExecution(cylinderInfos, doc, uidoc);
                    }
                    else if (cylinderInfos.Count == 1)
                    {
                        CylinderInfo firstCylinderInfo = cylinderInfos.FirstOrDefault();

                        cylinderInfosOfElements.Add(firstCylinderInfo);
                    }
                }
                // Trường hợp model gồm nhiều element ghép lại
                if (cylinderInfosOfElements.Count > 0)
                {
                    PrepareDataForExecution(cylinderInfosOfElements, doc, uidoc);

                    //var mergedList = MergeCylinderInfos(cylinderInfosOfElements);

                    //mergedList.Sort((a, b) => mergedList.First().TopPoint.DistanceTo(a.TopPoint)
                    //.CompareTo(mergedList.First().TopPoint.DistanceTo(b.TopPoint)));

                    //var result = GroupPointsForDistanceCalculation(mergedList);
                    //foreach (var group in result)
                    //{
                    //    CreateModelLine(doc, group.FirstOrDefault().TopPoint, group.LastOrDefault().TopPoint);
                    //}
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

        private void PrepareDataForExecution(List<CylinderInfo> cylinderInfos, Document doc, UIDocument uiDoc)
        {
            var mergedList = MergeCylinderInfos(cylinderInfos);
            CylinderInfo firstCylinderInfo = mergedList.First();
            mergedList.Sort((a, b) => firstCylinderInfo.TopPoint.DistanceTo(a.TopPoint)
            .CompareTo(firstCylinderInfo.TopPoint.DistanceTo(b.TopPoint)));

            //if (IsSpiralRailing(mergedList))
            if (GroupPointsOnSamePlane(mergedList).Count == 0)
            {
                var results = new List<List<CylinderInfo>> { mergedList };
                //string str = FlattenAndPrint(result);
                CalculateSpiralRailingLength(results, doc, uiDoc);
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
        /// Hàm vẽ Arc từ 3 điểm bất kỳ
        /// </summary>
        /// <param name="uiDoc"></param>
        /// <param name="doc"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void CreateModelArcFrom3Points(UIDocument uiDoc, Document doc, XYZ p1, XYZ p2, XYZ p3)
        {
            ModelCurve modelCurve = null;
            using (Transaction trans = new Transaction(doc, "Create Model Arc From 3 Points"))
            {
                trans.Start();

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
                    throw new InvalidOperationException("3 điểm thẳng hàng – không thể tạo cung.");

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
        private List<Solid> GetSolids(Element element, Document doc)
        {
            Options options = new Options();
            options.IncludeNonVisibleObjects = true;
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;
            GeometryElement elementGeo = element.get_Geometry(options);

            List<Solid> solids = new List<Solid>();

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
                    }
                }
            }
            return solids;
        }

        /// <summary>
        /// Lấy ra danh sách các CylinderInfo được tạo ra từ các CylindricalFace
        /// </summary>
        /// <param name="element"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private List<CylinderInfo> GetCylinderInfosFromElements(Element element, Document doc)
        {
            List<CylinderInfo> cylinderInfos = new List<CylinderInfo>();

            List<Solid> solids = GetSolids(element, doc);

            foreach (Solid solid in solids)
            {
                var tuple = GetGroupedFacesFromSolid(doc, solid);
                List<PlanarFace> planarFaces = tuple.Item1;
                List<CylindricalFace> cylindricalFaces = tuple.Item2;

                if (planarFaces.Count > 0 && cylindricalFaces.Count > 0)
                {
                    // Lấy ra những mặt trụ song song với Z và mặt phẳng vuông góc với Z
                    var listcylindricalFace = cylindricalFaces.Where(c => Math.Abs(Math.Abs(c.Axis.Z) - 1) < tolerance).ToList();
                    var listPlanarFace = planarFaces.Where(f => Math.Abs(Math.Abs(f.FaceNormal.Z) - 1) < tolerance).ToList();
                    if (listcylindricalFace.Count > 0)
                    {
                        if (listPlanarFace.Count == 2)
                        {
                            CylindricalFace cylindricalFace = listcylindricalFace.FirstOrDefault();
                            cylinderInfos.Add(CreateCylinderInfo(cylindricalFace, planarFaces.Max(f => f.Origin.Z), planarFaces.Min(f => f.Origin.Z)));
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
                                var tupleValue = GetHighestAndLowestZOfFace(face);
                                if (tupleValue.Max != 0 && tupleValue.Min != 0)
                                {
                                    cylinderInfos.Add(CreateCylinderInfo(face, tupleValue.Max, tupleValue.Min));
                                }
                            }
                        }
                    }
                }
            }
            return cylinderInfos;
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
        private (double Max, double Min) GetHighestAndLowestZOfFace(CylindricalFace face)
        {
            XYZ highestPoint = null;
            XYZ lowestPoint = null;

            // Lấy tất cả các điểm từ Face bằng cách tessellate nó.

            Mesh mesh = face.Triangulate();

            if (mesh == null || mesh.Vertices.Count == 0)
            {
                return (0, 0); // Face không có mesh hoặc không có đỉnh
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

            return (highestPoint.Z, lowestPoint.Z);
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

                XYZ newVector = XYZ.BasisZ.Multiply(1);
                XYZ p1 = point1.Add(newVector);
                XYZ p2 = point2.Add(newVector);

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
                Random random = new Random();

                // Tạo giá trị RGB ngẫu nhiên từ 0 đến 255
                byte red = (byte)random.Next(0, 256);
                byte green = (byte)random.Next(0, 256);
                byte blue = (byte)random.Next(0, 256);

                OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                ogs.SetProjectionLineColor(new Color(red, green, blue));

                doc.ActiveView.SetElementOverrides(modelCurve.Id, ogs);

                trans.Commit();
            }
        }

        /// <summary>
        /// Kiểm tra lan can xem có phải là lan can xoắn không
        /// </summary>
        /// <param name="cylinderInfos"></param>
        /// <returns></returns>
        private bool IsSpiralRailing(List<CylinderInfo> cylinderInfos)
        {
            bool kq = false;
            for (int i = 0; i < cylinderInfos.Count - 1; i++)
            {
                XYZ A = cylinderInfos[i].TopPoint;
                XYZ B = cylinderInfos[i + 1].TopPoint;
                bool isValid = A.Z < B.Z && A.X != B.X && A.Y != B.Y;
                if (!isValid)
                {
                    // Trường hợp A không nằm thấp hơn B, hoặc A trùng X, hoặc trùng Y
                    kq = true;
                    break;
                }
            }
            if (kq == false)
            {
                return true;
            }
            return false;
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
                        XYZ p3 = cylinderInfos[k].TopPoint;

                        // Tính vector pháp tuyến của mặt phẳng đi qua 3 điểm
                        XYZ v1 = p2 - p1;
                        XYZ v2 = p3 - p1;
                        XYZ normal = v1.CrossProduct(v2);
                        if (normal.IsZeroLength())
                            continue;

                        normal = normal.Normalize();

                        // Nếu pháp tuyến vuông góc với trục Z, nghĩa là mặt phẳng song song với trục Z
                        XYZ zAxis = XYZ.BasisZ;
                        double dot = Math.Abs(normal.DotProduct(zAxis));
                        if (dot > tolerance)
                            continue; // Không song song với Z

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
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Xử lý trường hợp có 1 danh sách trụ chéo và có 1 trụ thừa
        /// </summary>
        /// <param name="newResult"></param>
        /// <param name="remainCylinderInfo"></param>
        /// <param name="pointsAsDiagonalLine"></param>
        private void HandleSinglePoint(List<List<CylinderInfo>> newResult, List<CylinderInfo> remainCylinderInfo, List<CylinderInfo> pointsAsDiagonalLine)
        {
            // Trường hợp group thừa 1 trụ nằm ngoài khoảng của group chéo
            // kéo dài tia tạo bởi 2 trụ trong group chéo, nếu đi qua trụ thừa mà cao hơn thì lấy tại điểm cao hơn
            // nếu kéo dài tia mà cắt trụ thì lấy tại điểm thuộc tia và có chiều cao bằng trụ thừa
            var e = remainCylinderInfo.FirstOrDefault();

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

            //else if (ze == minZC)
            //{
            //    var otherPointsAsStraightLine = new List<CylinderInfo>();
            //    otherPointsAsStraightLine.Add(e);
            //    otherPointsAsStraightLine.Add(pointsAsDiagonalLine.FirstOrDefault(c => c.TopPoint.Z.Equals(minZC)));
            //    if (otherPointsAsStraightLine.Count > 0)
            //    {
            //        newResult.Add(otherPointsAsStraightLine);
            //    }
            //}
            //else if (ze == maxZC)
            //{
            //    var otherPointsAsStraightLine = new List<CylinderInfo>();
            //    otherPointsAsStraightLine.Add(e);
            //    otherPointsAsStraightLine.Add(pointsAsDiagonalLine.FirstOrDefault(c => c.TopPoint.Z.Equals(maxZC)));
            //    if (otherPointsAsStraightLine.Count > 0)
            //    {
            //        newResult.Add(otherPointsAsStraightLine);
            //    }
            //}
            else
            {
                if (zOnline < ze || zOnline > maxZC)
                {
                    // dùng phương trình đường thẳng để tìm ra điểm C thuộc đường thẳng AB và có cao độ là E
                    C = GetPointOnLine(pointsAsDiagonalLine[0].TopPoint, pointsAsDiagonalLine[1].TopPoint, e.TopPoint);
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

            // 1 group là 1 mặt phẳng
            foreach (var group in result)
            {
                var newGroup = CloneList(group);

                List<CylinderInfo> pointsAsDiagonalLine = GroupPointsAsDiagonalLine(newGroup);
                List<CylinderInfo> pointsAsStraightLine = GroupPointsAsStraightLine(newGroup, pointsAsDiagonalLine);

                HashSet<CylinderInfo> excluded = new HashSet<CylinderInfo>(pointsAsStraightLine.Concat(pointsAsDiagonalLine));

                List<CylinderInfo> remainCylinderInfo = newGroup.Where(x => !excluded.Contains(x)).ToList();

                // Xử lý trường hợp có 1 điểm thừa không thuộc danh sách thẳng và chéo
                if (remainCylinderInfo.Count == 1 && pointsAsDiagonalLine.Count > 0) // có tồn tại 1 điểm thừa
                {
                    HandleSinglePoint(newResult, remainCylinderInfo, pointsAsDiagonalLine);
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

            //string str = FlattenAndPrint(newResult);

            //string content = FormatNestedList(newResult);
            //TaskDialog.Show("Danh sách các nhóm", content);

            return newResult;
        }

        public string FormatNestedList(List<List<CylinderInfo>> A)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < A.Count; i++)
            {
                var group = A[i];
                builder.AppendLine($"Group {i + 1} (Count = {group.Count}):");

                if (group.Count > 0)
                {
                    builder.AppendLine(string.Join(Environment.NewLine, group.Select(p => p.TopPoint.ToString())));
                }

                builder.AppendLine(); // Dòng trống giữa các nhóm
            }

            return builder.ToString();
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

        private string FlattenAndPrint(List<List<CylinderInfo>> A)
        {
            // Gộp tất cả phần tử và loại trùng (Distinct)
            List<XYZ> uniquePoint = A.SelectMany(list => list)
                                   .Select(p => p.TopPoint)
                                   .Distinct()
                                   .ToList();

            // Ghép các chuỗi bằng dấu ;
            string result = string.Join(";", uniquePoint);

            return result;
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
            double t;

            if (Math.Abs(B.X - A.X) > tolerance)
            {
                t = (E.X - A.X) / (B.X - A.X);
            }
            else if (Math.Abs(B.Y - A.Y) > tolerance)
            {
                t = (E.Y - A.Y) / (B.Y - A.Y);
            }
            else
            {
                // Đường AB thẳng đứng (X và Y không đổi)
                // Kiểm tra xem E có cùng X, Y không
                if (Math.Abs(E.X - A.X) < tolerance && Math.Abs(E.Y - A.Y) < tolerance)
                {
                    return E.Z;
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
        /// <param name="listCheo"></param>
        /// <returns></returns>
        private List<CylinderInfo> GroupPointsAsStraightLine(List<CylinderInfo> group, List<CylinderInfo> listCheo)
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
            if (subList.Count > 2)
            {
                result.AddRange(subList);
                if (listCheo.Count > 0)
                {
                    ChangeGroupPointsAsDiagonalLine(listCheo, result);
                }

                double topZ = result.Max(c => c.TopPoint.Z);
                foreach (var c in result)
                {
                    c.TopPoint = SetOriginPoint(c.TopPoint, topZ);
                }
                // set lại origin của tất cả bằng với điểm cao nhất

                return SortInstancesAlongLine(result);
            }
            return result;
        }

        /// <summary>
        /// Sắp xếp các CylinderInfo theo thứ tự
        /// </summary>
        /// <param name="instances"></param>
        /// <returns></returns>
        private List<CylinderInfo> SortInstancesAlongLine(List<CylinderInfo> instances)
        {
            if (instances == null || instances.Count < 2)
                return instances;

            // Gốc là điểm đầu tiên
            XYZ origin = instances[0].TopPoint;

            // Dùng điểm thứ hai để xác định hướng
            XYZ direction = (instances[1].TopPoint - origin).Normalize();

            // Tính "t" cho từng instance rồi sắp xếp
            var sorted = instances
                .Select(inst => new
                {
                    Instance = inst,
                    T = (inst.TopPoint - origin).DotProduct(direction)
                })
                .OrderBy(x => x.T)
                .Select(x => x.Instance)
                .ToList();

            return sorted;
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
        /// <param name="listCheo"></param>
        /// <param name="listThang"></param>
        private void ChangeGroupPointsAsDiagonalLine(List<CylinderInfo> listCheo, List<CylinderInfo> listThang)
        {
            if (listThang.Any(c => listCheo.Contains(c)))
            {
                CylinderInfo commonInstance = null;
                foreach (var p1 in listThang)
                {
                    foreach (var p2 in listCheo)
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
                    int index = listCheo.IndexOf(commonInstance);
                    var newInstance = commonInstance.Clone();

                    listCheo[index] = newInstance; // thay thế trực tiếp
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
            for (int i = 3; i < group.Count; i++)
            {
                if (ArePointsCollinear(group[i - 3], group[i - 2], group[i - 1], group[i]))
                {
                    mySet.Add(group[i]);
                    mySet.Add(group[i - 1]);
                    mySet.Add(group[i - 2]);
                    mySet.Add(group[i - 3]);
                }
            }
            if (mySet.Count >= 4)
            {
                result.AddRange(mySet);
                result.Sort((c1, c2) => c1.TopPoint.Z.CompareTo(c2.TopPoint.Z));

                return result;
            }
            return result;
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
            return (Math.Abs(c1.TopPoint.Z - c2.TopPoint.Z) < radius * 2 || Math.Abs(c1.BottomPoint.Z - c2.BottomPoint.Z) < tolerance);
        }

        /// <summary>
        /// Kiểm tra xem 4 điểm có tạo thành 1 đường chéo hay không, sai số cho phép là các vector từ các điểm tạo thành 1 góc nhỏ hơn 5 độ
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="c3"></param>
        /// <param name="c4"></param>
        /// <returns></returns>
        private bool ArePointsCollinear(CylinderInfo c1, CylinderInfo c2, CylinderInfo c3, CylinderInfo c4)
        {
            double newTolerance = 0.0872; // 5 độ

            XYZ A = c1.TopPoint;
            XYZ B = c2.TopPoint;
            XYZ C = c3.TopPoint;
            XYZ D = c4.TopPoint;

            double radius = c1.Radius;

            if (Math.Abs(A.Z - B.Z) > radius * 2 && Math.Abs(B.Z - C.Z) > radius * 2)
            {
                XYZ AB = (B - A).Normalize();
                XYZ AC = (C - A).Normalize();
                XYZ AD = (D - A).Normalize();

                // Nếu vector AB và AC không cùng phương -> không thẳng hàng
                if (!IsParallel(AB, AC, newTolerance))
                    return false;

                // Nếu vector AB và AD không cùng phương -> không thẳng hàng
                if (!IsParallel(AB, AD, newTolerance))
                    return false;

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

        private bool IsParallel(XYZ v1, XYZ v2, double newTolerance)
        {
            var cross = v1.CrossProduct(v2);
            return cross.GetLength() < newTolerance;
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