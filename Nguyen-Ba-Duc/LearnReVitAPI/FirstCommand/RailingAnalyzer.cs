using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

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
                    List<CylinderInfo> cylinderInfos = GetOrigins(element, doc);

                    // Trường hộp model là 1 khối thống nhất
                    if (cylinderInfos.Count > 1)
                    {
                        var list = MergeCylinderInfos(cylinderInfos);
                        CylinderInfo firstCylinderInfo = list.First();
                        list.Sort((a, b) => firstCylinderInfo.TopPoint.DistanceTo(a.TopPoint)
                        .CompareTo(firstCylinderInfo.TopPoint.DistanceTo(b.TopPoint)));

                        GroupPointsOnPlaneParallelToZ(list);
                        //points.Sort((a, b) => points.First().DistanceTo(a).CompareTo(points.First().DistanceTo(b)));
                        //for (int i = 0; i < points.Count - 1; i++)
                        //{
                        //    CreateModelLine(doc, points[i], points[i + 1]);
                        //}
                    }
                    else if (cylinderInfos.Count == 1)
                    {
                        CylinderInfo firstCylinderInfo = cylinderInfos.FirstOrDefault();

                        //if (!cylinderInfosOfElements.Any(p => p.TopPoint.IsAlmostEqualTo(firstCylinderInfo.TopPoint, tolerance)))
                        //{
                        //    cylinderInfosOfElements.Add(firstCylinderInfo);
                        //}
                        cylinderInfosOfElements.Add(firstCylinderInfo);

                        //XYZ pt = points.FirstOrDefault();

                        //if (!pointsOfElements.Any(p => p.IsAlmostEqualTo(pt, tolerance)))
                        //{
                        //    pointsOfElements.Add(pt);
                        //}
                    }
                }
                // Trường hợp model gồm nhiều element ghép lại
                if (cylinderInfosOfElements.Count > 0)
                {
                    var list = MergeCylinderInfos(cylinderInfosOfElements);

                    list.Sort((a, b) => list.First().TopPoint.DistanceTo(a.TopPoint)
                    .CompareTo(list.First().TopPoint.DistanceTo(b.TopPoint)));

                    GroupPointsOnPlaneParallelToZ(list);
                }
                //for (int i = 0; i < pointsOfElements.Count - 1; i++)
                //{
                //    CreateModelLine(doc, pointsOfElements[i], pointsOfElements[i + 1]);
                //}
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Người dùng nhấn ESC
                TaskDialog.Show("Thông báo", "Command bị hủy bởi người dùng.");
                return Result.Cancelled;
            }
            return Result.Succeeded;
            //Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

            //if (r != null)
            //{
            //    ElementId elementId = r.ElementId;
            //    Element element = doc.GetElement(elementId);
            //    GetFacesOnGeometry(element, doc);

            //    return Result.Succeeded;
            //}
            //return Result.Failed;
        }

        // Kiểm tra xem trong danh sách hình trụ của tất cả các element có tồn tại hình trụ có cùng top point của 1 bằng bottom của 2 không và ngược lại,
        // nếu bằng thì merge chúng lại, lấy maxtop và min bottom
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
                    //if (current.TopPoint.IsAlmostEqualTo(other.BottomPoint, tolerance) || current.BottomPoint.IsAlmostEqualTo(other.TopPoint, tolerance))
                    //{
                    //    CollectConnected(other, input, group, visited);
                    //}
                    if (Math.Abs(current.TopPoint.X - other.TopPoint.X) < tolerance && Math.Abs(current.TopPoint.Y - other.TopPoint.Y) < tolerance)
                    {
                        CollectConnected(other, input, group, visited);
                    }
                }
            }
        }

        private List<CylinderInfo> GetOrigins(Element element, Document doc)
        {
            Options options = new Options();
            //options.View = doc.ActiveView;
            options.IncludeNonVisibleObjects = true;
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;
            GeometryElement elementGeo = element.get_Geometry(options);

            List<XYZ> points = new List<XYZ>();

            List<CylinderInfo> cylinderInfos = new List<CylinderInfo>();

            foreach (GeometryObject geometryObj in elementGeo)
            {
                if (geometryObj is Solid solid)
                {
                    if (solid.Faces.Size > 0 && solid.Volume > 0)
                    {
                        List<PlanarFace> planarFaces = new List<PlanarFace>();
                        List<CylindricalFace> cylindricalFaces = new List<CylindricalFace>();
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
                        if (planarFaces.Count == 2 && cylindricalFaces.Count > 0)
                        {
                            CylindricalFace cylindricalFace = cylindricalFaces.First();

                            if (AreVectorsParallel(cylindricalFace.Axis, XYZ.BasisZ))
                            {
                                CylinderInfo cylinderInfo = new CylinderInfo();

                                cylinderInfo.Radius = GetRadius(cylindricalFace);
                                cylinderInfo.TopPoint = SetOriginPoint(cylindricalFace.Origin, planarFaces.Max(f => f.Origin.Z));
                                cylinderInfo.BottomPoint = SetOriginPoint(cylindricalFace.Origin, planarFaces.Min(f => f.Origin.Z));
                                cylinderInfo.Volume = solid.Volume;
                                cylinderInfos.Add(cylinderInfo);
                            }
                        }
                    }
                }
                else if (geometryObj is GeometryInstance geomInstance)
                {
                    GeometryElement instanceGeometry = geomInstance.GetInstanceGeometry();
                    foreach (GeometryObject geometryObject in instanceGeometry)
                    {
                        if (geometryObject is Solid nestedSolid)
                        {
                            if (nestedSolid.Faces.Size > 0 && nestedSolid.Volume > 0)
                            {
                                List<PlanarFace> planarFaces = new List<PlanarFace>();
                                List<CylindricalFace> cylindricalFaces = new List<CylindricalFace>();
                                foreach (Face face in nestedSolid.Faces)
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
                                if (planarFaces.Count == 2 && cylindricalFaces.Count > 0)
                                {
                                    CylindricalFace cylindricalFace = cylindricalFaces.First();

                                    if (AreVectorsParallel(cylindricalFace.Axis, XYZ.BasisZ))
                                    {
                                        CylinderInfo cylinderInfo = new CylinderInfo();

                                        cylinderInfo.Radius = GetRadius(cylindricalFace);
                                        cylinderInfo.TopPoint = SetOriginPoint(cylindricalFace.Origin, planarFaces.Max(f => f.Origin.Z));
                                        cylinderInfo.BottomPoint = SetOriginPoint(cylindricalFace.Origin, planarFaces.Min(f => f.Origin.Z));
                                        cylinderInfo.Volume = nestedSolid.Volume;
                                        cylinderInfos.Add(cylinderInfo);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //if (cylindricalFaces.Count > 0)
            //{
            //    XYZ firstOrigin = new XYZ();
            //    foreach (var face in cylindricalFaces)
            //    {
            //        if (AreVectorsParallel(face.Axis, XYZ.BasisZ))
            //        {
            //            if (!face.Origin.IsAlmostEqualTo(firstOrigin))
            //            {
            //                points.Add(face.Origin);
            //                firstOrigin = face.Origin;
            //            }
            //        }
            //    }
            //}
            return cylinderInfos;
        }

        private double GetRadius(CylindricalFace face)
        {
            CylindricalSurface s = face.GetSurface() as CylindricalSurface;
            double radius = s.Radius;
            return radius;
        }

        private bool AreVectorsParallel(XYZ vector1, XYZ vector2)
        {
            return vector1.CrossProduct(vector2).GetLength() < tolerance;
        }

        private void CreateModelLine(Document doc, XYZ point1, XYZ point2)
        {
            using (Transaction trans = new Transaction(doc, "Create Model Line with Auto Plane"))
            {
                trans.Start();

                //XYZ pt1 = new XYZ(point1.X, point1.Y, 0);
                //XYZ pt2 = new XYZ(point2.X, point2.Y, 0);

                XYZ newVector = -XYZ.BasisY.Multiply(5);
                XYZ p1 = point1.Add(newVector);
                XYZ p2 = point2.Add(newVector);
                //XYZ p1 = transform.OfPoint(pt1);
                //XYZ p2 = transform.OfPoint(pt2);

                Line line = Line.CreateBound(p1, p2);
                XYZ direction = (p1 - p2).Normalize();

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
                doc.Create.NewModelCurve(line, sketchPlane);
                trans.Commit();
            }
        }

        private List<List<CylinderInfo>> GroupPointsOnPlaneParallelToZ(List<CylinderInfo> cylinderInfos)
        {
            // Nhóm các điểm thuộc cùng 1 mặt phẳng lại với nhau
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
                            if (isPointOnPlane(plane, pt.TopPoint))
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

            // Trong các điểm cùng 1 mặt phẳng tìm ra chia các điểm bằng nhau thành 1 nhóm, các điểm tạo thành đường chéo làm 1 nhóm
            List<List<CylinderInfo>> newResult = new List<List<CylinderInfo>>();
            foreach (var group in result)
            {
                List<CylinderInfo> listThang = GroupEqualValues(group);
                List<CylinderInfo> listCheo = GroupPointMakeLine(group);

                // Gộp tất cả các phần tử trong B và C vào một HashSet để tìm kiếm nhanh
                //HashSet<CylinderInfo> excluded = new HashSet<CylinderInfo>(listThang.SelectMany(x => x)
                //                                               .Concat(listCheo.SelectMany(x => x)));
                HashSet<CylinderInfo> excluded = new HashSet<CylinderInfo>(listThang.Concat(listCheo));

                // Lọc ra các phần tử trong A không nằm trong excluded
                List<CylinderInfo> remainCylinderInfo = group.Where(x => !excluded.Contains(x)).ToList();

                // Xử lý trường hợp có 1 điểm thừa không thuộc danh sách thẳng và chéo
                if (remainCylinderInfo.Count == 1 && listCheo.Count > 0) // có tồn tại 1 điểm thừa
                {
                    // Xử lý trường hợp 1 group có nhiều điểm thừa

                    // Sử lý trường hợp group thừa 1-2 trụ nằm ngoài khoảng của group chéo và ngang
                    // Lấy điểm xa nhât, kéo dài tia tạo bởi first và last, nếu đi qua trụ mà cao hơn thì lấy tại điểm cao hơn
                    // nếu kéo dài tia mà cắt trụ thì lấy tại điểm thuộc tia và có chiều cao bằng trụ
                    var e = remainCylinderInfo.FirstOrDefault();

                    //var listT = listThang.FirstOrDefault();

                    double ze = e.TopPoint.Z;
                    double maxZC = listCheo.Max(x => x.TopPoint.Z);
                    double minZC = listCheo.Min(x => x.TopPoint.Z);

                    double zOnline = GetZOnLine(listCheo[0].TopPoint, listCheo[1].TopPoint, e.TopPoint);
                    XYZ C = new XYZ();

                    if (zOnline < ze || zOnline > maxZC)
                    {
                        C = GetPointOnLine(listCheo[0].TopPoint, listCheo[1].TopPoint, e.TopPoint);
                    }
                    else if (zOnline > ze && zOnline < minZC)
                    {
                        C = new XYZ(e.TopPoint.X, e.TopPoint.Y, zOnline);
                    }

                    if (C != null)
                    {
                        CylinderInfo cylinderInfo = new CylinderInfo();
                        cylinderInfo.TopPoint = C;
                        List<CylinderInfo> newListT = new List<CylinderInfo>();
                        if (C.Z.Equals(e.TopPoint.Z))
                        {
                            newListT.Add(e);
                            newListT.Add(cylinderInfo);
                            if (C.Z < minZC)
                            {
                                listCheo.Insert(0, cylinderInfo);
                            }
                            else if (C.Z > maxZC)
                            {
                                listCheo.Add(cylinderInfo);
                            }
                        }
                        else
                        {
                            if (C.Z < minZC)
                            {
                                listCheo.Insert(0, cylinderInfo);
                            }
                            else if (C.Z > maxZC)
                            {
                                listCheo.Add(cylinderInfo);
                            }
                        }
                        if (newListT.Count > 0)
                        {
                            newResult.Add(newListT);
                        }
                        newResult.Add(listCheo);
                    }
                }
                // không tồn tại điểm thừa

                // kiểm tra xem nhóm chéo và thẳng có kiểm chung không
                if (listThang.Count > 0 && listCheo.Count > 0 && !listThang.Any(c => listCheo.Contains(c))) // nếu không tồn tại điểm chung
                {
                    double zT = listThang.First().TopPoint.Z;
                    double maxZC = listCheo.Max(c => c.TopPoint.Z);
                    double minZC = listCheo.Min(c => c.TopPoint.Z);

                    XYZ C = GetPointOnLine(listCheo[0].TopPoint, listCheo[1].TopPoint, listThang.First().TopPoint);
                    if (C != null)
                    {
                        CylinderInfo cylinderInfo = new CylinderInfo();
                        cylinderInfo.TopPoint = C;

                        if (zT < minZC)
                        {
                            listThang.Add(cylinderInfo);
                            listCheo.Insert(0, cylinderInfo);
                        }
                        else if (zT > maxZC)
                        {
                            listThang.Insert(0, cylinderInfo);
                            listCheo.Add(cylinderInfo);
                        }
                    }
                }
                // Xử lý trường hợp đã thêm list chéo rồi mà giờ lại thêm list chéo nữa
                if (listThang.Count > 0)
                {
                    newResult.Add(listThang);
                }
                if (!newResult.Contains(listCheo) && listCheo.Count > 0)
                {
                    newResult.Add(listCheo);
                }
            }

            // Trường hợp thừa 1-2 điểm không tạo thành 1 mặt phẳng
            HashSet<CylinderInfo> allGroupedPoints = new HashSet<CylinderInfo>(result.SelectMany(g => g));
            var remainingcylinderInfos = cylinderInfos.Where(p => !allGroupedPoints.Any(q => q.Equals(p))).ToList();
            List<CylinderInfo> usedCylinderInfo = new List<CylinderInfo>();
            if (remainingcylinderInfos.Count > 0)
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
                            //if (group.Count < 3) continue;
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
                        //newCylinderInfo.BottomPoint = new XYZ(E.X, E.Y, cylinderInfoA.BottomPoint.Z);

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

                            // Xử lý trường hợp nếu có thêm điểm thừa khác thuộc mặt phẳng thì kiểm trả xem 3 điểm đó thẳng hàng không
                            // thẳng hàng thì thêm vào, còn không chỉ đó là 3 điểm lệch nhau(trường hợp này chưa có model nào giống vậy để xử lý)
                            if (isPointOnPlane(aePlane, info.TopPoint))
                            {
                                newGroup.Add(info);
                                usedCylinderInfo.Add(info);
                            }
                        }

                        newResult.Add(newGroup);
                    }
                }
            }

            string str = FlattenAndPrint(newResult);

            return newResult;
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

        private double GetZOnLine(XYZ A, XYZ B, XYZ E)
        {
            double yE = E.Y;
            double dY = B.Y - A.Y;

            if (Math.Abs(dY) < tolerance)
            {
                return Math.Abs(yE - B.Y) < tolerance ? E.Z : 0;
            }
            double t = (yE - A.Y) / dY;
            double z = A.Z + (B.Z - A.Z) * t;

            return z;
        }

        private List<CylinderInfo> GroupEqualValues(List<CylinderInfo> group)
        {
            HashSet<CylinderInfo> mySet = new HashSet<CylinderInfo>();
            var result = new List<CylinderInfo>();
            for (int i = 0; i < group.Count - 1; i++)
            {
                for (int j = i + 1; j < group.Count; j++)
                {
                    if (KiemTra2DiemThangHang(group[i], group[j], group[j].Radius))
                    {
                        mySet.Add(group[i]);
                        mySet.Add(group[j]);
                    }
                }
            }
            if (mySet.Count > 2)
            {
                result.AddRange(mySet);
                double topZ = result.Max(c => c.TopPoint.Z);
                foreach (var c in result)
                {
                    c.TopPoint = SetOriginPoint(c.TopPoint, topZ);
                }
                // set lại origin của tất cả bằng với điểm cao nhất

                XYZ firstPoint = result.FirstOrDefault().TopPoint;
                result.Sort((a, b) => firstPoint.DistanceTo(a.TopPoint).CompareTo(firstPoint.DistanceTo(b.TopPoint)));

                return result;
            }
            return result;
        }

        private List<CylinderInfo> GroupPointMakeLine(List<CylinderInfo> group)
        {
            HashSet<CylinderInfo> mySet = new HashSet<CylinderInfo>();
            var result = new List<CylinderInfo>();
            for (int i = 2; i < group.Count; i++)
            {
                if (ArePointsCollinear(group[i], group[i - 1], group[i - 2]))
                {
                    mySet.Add(group[i]);
                    mySet.Add(group[i - 1]);
                    mySet.Add(group[i - 2]);
                }
            }
            if (mySet.Count > 2)
            {
                result.AddRange(mySet);

                XYZ firstPoint = result.FirstOrDefault().TopPoint;
                result.Sort((a, b) => firstPoint.DistanceTo(a.TopPoint).CompareTo(firstPoint.DistanceTo(b.TopPoint)));

                return result;
            }
            return result;
        }

        private bool KiemTra2DiemThangHang(CylinderInfo c1, CylinderInfo c2, double radius)
        {
            return (Math.Abs(c1.TopPoint.Z - c2.TopPoint.Z) < radius * 2 || Math.Abs(c1.BottomPoint.Z - c2.BottomPoint.Z) < tolerance);
        }

        private bool ArePointsCollinear(CylinderInfo c1, CylinderInfo c2, CylinderInfo c3, double newTolerance = 0.0349)
        {
            XYZ top1 = c1.TopPoint;
            XYZ top2 = c2.TopPoint;
            XYZ top3 = c3.TopPoint;

            //XYZ bottom1 = c1.BottomPoint;
            //XYZ bottom2 = c2.BottomPoint;
            //XYZ bottom3 = c3.BottomPoint;

            double radius = c1.Radius;

            if (Math.Abs(top1.Z - top2.Z) > radius * 2)
            {
                XYZ vTop1 = (top2 - top1).Normalize();
                XYZ vTop2 = (top3 - top1).Normalize();

                //XYZ vBot1 = (bottom2 - bottom1).Normalize();
                //XYZ vBot2 = (bottom3 - bottom1).Normalize();
                // Tính tích có hướng (cross product)
                XYZ crossTop = vTop1.CrossProduct(vTop2);
                //XYZ crossBot = vBot1.CrossProduct(vBot2);

                // Nếu độ dài vector tích có hướng nhỏ hơn newTolerance(quy đổi từ 2 độ sang), các vector cùng phương ⇒ 3 điểm thẳng hàng
                return crossTop.GetLength() < newTolerance;
            }
            return false;
        }

        private XYZ SetOriginPoint(XYZ p, double z)
        {
            var result = new XYZ(p.X, p.Y, z);
            return result;
        }

        private bool IsSameZ(XYZ p1, XYZ p2)
        {
            return Math.Abs(p1.Z - p2.Z) < tolerance;
        }

        private XYZ GetProjectedPoint(Plane plane, XYZ point)
        {
            XYZ planeOrigin = plane.Origin;
            XYZ planeNormal = plane.Normal.Normalize();
            XYZ pointToOrigin = point - planeOrigin;

            // Tính khoảng cách từ điểm đến mặt phẳng (dọc theo pháp tuyến)
            double distance = pointToOrigin.DotProduct(planeNormal);

            // Chiếu điểm xuống mặt phẳng
            return point - distance * planeNormal;
        }

        private bool isPointOnPlane(Plane plane, XYZ point)
        {
            double distance = plane.Normal.DotProduct(point - plane.Origin);
            return Math.Abs(distance) < tolerance;
        }
    }

    public class CylinderInfo
    {
        public double Radius { get; set; }
        public XYZ TopPoint { get; set; }
        public XYZ BottomPoint { get; set; }

        public double Volume { get; set; }
    }
}