using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using FirstCommand.View;
using Microsoft.SqlServer.Server;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateDimCommand : IExternalCommand
    {
        private double tolerance = 0.00001;
        private double feetToMm = 304.8;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            ViewComboboxWindow viewWindow = new ViewComboboxWindow(doc);
            if (viewWindow.ShowDialog() == true)
            {
                double offset = viewWindow.Offset;
                double sectionDepth = viewWindow.SectionDepth;

                Reference pickedRef = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick a wall");

                if (pickedRef != null)
                {
                    Element selectedElem = doc.GetElement(pickedRef);
                    if (selectedElem is Wall)
                    {
                        Wall selectedWall = selectedElem as Wall;

                        List<FamilyInstance> familyInstances = GetInstanceOnWall(doc, selectedWall);
                        CreateSectionView(doc, familyInstances, selectedWall, sectionDepth);

                        PrepareForDimmension(doc, familyInstances, selectedWall.Id, offset);

                        return Result.Succeeded;
                    }
                    return Result.Failed;
                }
                return Result.Failed;
            }
            TaskDialog.Show("Note", "Element has been selected is not a wall");
            return Result.Failed;
        }

        //Hàm thực hiện transaction
        public void RunTransaction(Document doc, string transactionName, Action<Transaction> action)
        {
            using (Transaction trans = new Transaction(doc, transactionName))
            {
                trans.Start();

                FailureHandlingOptions options = trans.GetFailureHandlingOptions();
                options.SetFailuresPreprocessor(new WarningSuppressor());
                trans.SetFailureHandlingOptions(options);

                action(trans); // Thực thi hành động trong Transaction

                trans.Commit();
            }
        }

        //________________
        //Start-- Add Dimmention

        private void PrepareDimForSectionView(List<FamilyInstance> listInstances, Wall selectedWall, Document doc, Autodesk.Revit.DB.View view, double offset)
        {
            List<Face> listFaces = new List<Face>();
            List<Face> listFacesToChoose = new List<Face>();

            foreach (Face face in GetFacesOnGeometry(selectedWall))
            {
                if (IsFaceNormalEqualToZ(face))
                {
                    listFaces.Add(face);
                }
            }

            listFaces.Sort((a, b) => GetMidPointOfFace(a).Z.CompareTo(GetMidPointOfFace(b).Z));
            listFacesToChoose.Add(listFaces.First());
            double firstZ = GetMidPointOfFace(listFaces.First()).Z;
            double lastZ = GetMidPointOfFace(listFaces.Last()).Z;
            listFacesToChoose.Add(listFaces.Last());

            foreach (Face face in listFaces)
            {
                foreach (FamilyInstance instance in listInstances)
                {
                    BoundingBoxXYZ instanceBbox = instance.get_BoundingBox(null);
                    if (IsInBoundingBox(GetMidPointOfFace(face), instanceBbox) && (GetMidPointOfFace(face).Z != firstZ && GetMidPointOfFace(face).Z != lastZ))
                    {
                        listFacesToChoose.Add(face);
                    }
                }
            }
            listFacesToChoose.Sort((a, b) => GetMidPointOfFace(a).Z.CompareTo(GetMidPointOfFace(b).Z));
            BoundingBoxXYZ bbox = listInstances.FirstOrDefault().get_BoundingBox(null);
            XYZ midPoint = (bbox.Min + bbox.Max) / 2;
            RunTransToCreateDim(doc, listFacesToChoose, view, offset, midPoint, view.UpDirection, view.RightDirection);
        }

        private void RunTransToCreateDim(Document doc, List<Face> listFaces, Autodesk.Revit.DB.View view, double offset, XYZ point, XYZ vector, XYZ direction)
        {
            RunTransaction(doc, "Create Dimension", (Transaction t) =>
            {
                ReferenceArray referenceArray = new ReferenceArray();

                for (int i = 0; i < listFaces.Count(); i++)
                {
                    referenceArray.Append(listFaces[i].Reference);
                }
                CreateDim(GetOffset(point, direction, offset), doc, vector, referenceArray, view);
                referenceArray.Clear();
                referenceArray.Append(listFaces[0].Reference);
                referenceArray.Append(listFaces[listFaces.Count() - 1].Reference);
                if (listFaces.Count > 2)
                {
                    CreateDim(GetOffset(point, direction, offset + 5), doc, vector, referenceArray, view);
                }
            });
        }

        private void CreateDim(XYZ point, Document doc, XYZ vector, ReferenceArray referenceArray, Autodesk.Revit.DB.View view)
        {
            Line dimLine = Line.CreateUnbound(point, vector);

            Dimension newDim = doc.Create.NewDimension(view, dimLine, referenceArray);
        }

        private XYZ GetMidPointOfFace(Face face)
        {
            BoundingBoxUV bbox = face.GetBoundingBox();
            UV midUV = new UV((bbox.Min.U + bbox.Max.U) / 2, (bbox.Min.V + bbox.Max.V) / 2);
            return face.Evaluate(midUV);
        }

        private bool IsInBoundingBox(XYZ point, BoundingBoxXYZ bbox)
        {
            XYZ bboxMin = bbox.Min;
            XYZ bboxMax = bbox.Max;
            bool result = false;

            if (point.X >= bboxMin.X && point.X <= bboxMax.X &&
                point.Y >= bboxMin.Y && point.Y <= bboxMax.Y &&
                point.Z >= bboxMin.Z && point.Z <= bboxMax.Z)
            {
                result = true;
            }

            return result;
        }

        private bool IsFaceNormalEqualToZ(Face face)
        {
            if (face is PlanarFace)
            {
                PlanarFace pf = face as PlanarFace;

                if (pf.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ, tolerance) || pf.FaceNormal.IsAlmostEqualTo(-XYZ.BasisZ, tolerance))
                {
                    return true;
                }
            }
            return false;
        }

        private List<FamilyInstance> GetInstanceOnSection(Document doc, ElementId viewId, Wall selectedWall)
        {
            List<FamilyInstance> familyInstances = new FilteredElementCollector(doc, viewId)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .ToList();
            List<FamilyInstance> listFamilyIntances = new List<FamilyInstance>();
            foreach (FamilyInstance instance in familyInstances)
            {
                if (instance.Host != null && instance.Host.Id == selectedWall.Id)
                {
                    listFamilyIntances.Add(instance);
                }
            }
            return listFamilyIntances;
        }

        private void CreateSectionView(Document doc, List<FamilyInstance> familyInstances, Wall wall, double sectionDepth)
        {
            double height = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
            foreach (FamilyInstance familyInstance in familyInstances)
            {
                BoundingBoxXYZ boundingBox = familyInstance.get_BoundingBox(null);

                XYZ familyCenter = (boundingBox.Max + boundingBox.Min) / 2;
                XYZ wallOrientation = wall.Orientation;

                Transform sectionTransform = Transform.Identity;
                sectionTransform.Origin = familyCenter;
                sectionTransform.BasisX = wallOrientation;
                sectionTransform.BasisY = XYZ.BasisZ;
                sectionTransform.BasisZ = wallOrientation.CrossProduct(XYZ.BasisZ);

                // Kích thước mặt cắt
                double sectionWidth = 10;   // Chiều rộng
                //double sectionDepth = 6; // Độ sâu
                double sectionHeight = height;  // Chiều cao

                BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
                sectionBox.Transform = sectionTransform;
                sectionBox.Min = new XYZ(-sectionWidth, -sectionHeight, 0);
                sectionBox.Max = new XYZ(sectionWidth, sectionHeight, sectionDepth);

                ViewFamilyType sectionType = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .FirstOrDefault(x => x.ViewFamily == ViewFamily.Section);
                if (sectionType == null)
                {
                    TaskDialog.Show("Lỗi", "Không tìm thấy ViewFamilyType cho Section.");
                }
                RunTransaction(doc, "Create ViewSection", (Transaction t) =>
                {
                    ViewSection sectionView = ViewSection.CreateSection(doc, sectionType.Id, sectionBox);
                });
            }
        }

        private void PrepareForDimmension(Document doc, List<FamilyInstance> familyInstances, ElementId elementId, double offset)
        {
            List<Face> listFaces = new List<Face>();
            List<Face> listFacesOnWall = new List<Face>();

            Wall wall = doc.GetElement(elementId) as Wall;
            LocationCurve wallLoc = wall.Location as LocationCurve;
            Line wallLine = wallLoc.Curve as Line;
            XYZ start = wallLine.GetEndPoint(0);

            listFaces.AddRange(GetFaceOnWall(wall, wallLine, start).startEnd);
            listFacesOnWall = GetFaceOnWall(wall, wallLine, start).allFaces;

            List<FamilyInstanceInfo> listFamilyInstanceInfos = new List<FamilyInstanceInfo>();
            listFamilyInstanceInfos = GetListFamilyInstaneInfo(familyInstances, listFacesOnWall, start, wallLine);

            IList<Element> intersectElements = ListIntersectElements(doc, wall);
            listFamilyInstanceInfos.AddRange(GetListFaces(intersectElements, wallLine, start));

            CreateDimForEachView(doc, wall, listFaces, listFamilyInstanceInfos, start, wallLine, offset);
        }

        private List<FamilyInstanceInfo> GetListFamilyInstaneInfo(List<FamilyInstance> familyInstances, List<Face> listFacesOnWall, XYZ start, Line wallLine)
        {
            List<FamilyInstanceInfo> listFamilyInstanceInfos = new List<FamilyInstanceInfo>();
            foreach (FamilyInstance instance in familyInstances)
            {
                FamilyInstanceInfo familyInstanceInfo = new FamilyInstanceInfo();
                List<Face> facePairs = new List<Face>();
                BoundingBoxXYZ bbox = instance.get_BoundingBox(null);
                foreach (Face face in listFacesOnWall)
                {
                    if (IsInBoundingBox(GetMidPointOfFace(face), bbox))
                    {
                        facePairs.Add(face);
                    }
                }
                facePairs.Sort((a, b) => start.DistanceTo(ProjectPointOntoLine(GetMidPointOfFace(a), wallLine))
                            .CompareTo(start.DistanceTo((ProjectPointOntoLine(GetMidPointOfFace(b), wallLine)))));
                familyInstanceInfo.facePairs = facePairs;
                familyInstanceInfo.couplePoints = GetProjectPointOnLocationCurve(familyInstanceInfo.facePairs, wallLine);
                listFamilyInstanceInfos.Add(familyInstanceInfo);
            }
            return listFamilyInstanceInfos;
        }

        private (List<Face> startEnd, List<Face> allFaces) GetFaceOnWall(Wall wall, Line wallLine, XYZ start)
        {
            List<Face> listFaces = new List<Face>();
            List<Face> listFacesOnWall = new List<Face>();
            foreach (Face face in GetFacesOnGeometry(wall))
            {
                if (face != null && IsFaceNormalWithLine(face, wallLine))
                {
                    listFacesOnWall.Add(face);
                }
            }
            listFacesOnWall.Sort((a, b) => start.DistanceTo(ProjectPointOntoLine(GetMidPointOfFace(a), wallLine))
                            .CompareTo(start.DistanceTo((ProjectPointOntoLine(GetMidPointOfFace(b), wallLine)))));
            listFaces.Add(listFacesOnWall.First());
            listFaces.Add(listFacesOnWall.Last());
            return (listFaces, listFacesOnWall);
        }

        private void CreateDimForEachView(Document doc, Wall wall, List<Face> listFaces, List<FamilyInstanceInfo> listFamilyInstanceInfos, XYZ start, Line wallLine, double offset)
        {
            var views = new FilteredElementCollector(doc)
                 .OfClass(typeof(Autodesk.Revit.DB.View))
                 .OfType<Autodesk.Revit.DB.View>()
                 .Where(v => !v.IsTemplate)
                 .ToList();
            foreach (var view in views)
            {
                if (view.ViewType == ViewType.Section)
                {
                    List<FamilyInstance> listIntances = GetInstanceOnSection(doc, view.Id, wall);
                    PrepareDimForSectionView(listIntances, wall, doc, view, offset);
                }
                else if (view.ViewType == ViewType.Elevation)
                {
                    List<FamilyInstanceInfo> newListFamilyInstanceInfos1 = new List<FamilyInstanceInfo>();
                    newListFamilyInstanceInfos1 = listFamilyInstanceInfos;
                    List<Face> newListFaces1 = listFaces.ToList();
                    //List<Face> newListFaces1 = listFaces;
                    newListFaces1.AddRange(GetFaceValidOnInstance(newListFamilyInstanceInfos1, wallLine));

                    newListFaces1.Sort((a, b) => start.DistanceTo(ProjectPointOntoLine(GetMidPointOfFace(a), wallLine))
                                    .CompareTo(start.DistanceTo((ProjectPointOntoLine(GetMidPointOfFace(b), wallLine)))));
                    RunTransToCreateDim(doc, newListFaces1, view, offset, start, view.RightDirection, -XYZ.BasisZ);
                }
                else if (view is ViewPlan viewPlan && view.ViewType == ViewType.FloorPlan)
                {
                    List<Face> newListFaces = listFaces.ToList();
                    List<FamilyInstanceInfo> newListFamilyInstanceInfos = new List<FamilyInstanceInfo>();
                    foreach (FamilyInstanceInfo info in listFamilyInstanceInfos)
                    {
                        if (IsInViewRange(viewPlan, info.facePairs[0]))
                        {
                            newListFamilyInstanceInfos.Add(info);
                        }
                    }
                    newListFaces.AddRange(GetFaceValidOnInstance(newListFamilyInstanceInfos, wallLine));
                    newListFaces.Sort((a, b) => start.DistanceTo(ProjectPointOntoLine(GetMidPointOfFace(a), wallLine))
                            .CompareTo(start.DistanceTo((ProjectPointOntoLine(GetMidPointOfFace(b), wallLine)))));

                    RunTransToCreateDim(doc, newListFaces, view, offset, start, wallLine.Direction.Normalize(), wall.Orientation);
                }
            }
        }

        private bool IsInViewRange(ViewPlan viewPlan, Face face)
        {
            double elevation = 0;
            Level level = viewPlan.GenLevel;
            if (level != null)
            {
                elevation = level.Elevation;
            }
            ViewRangeValue viewRangeValue = GetViewRange(viewPlan, elevation);

            var topAndBottomZ = GetTopAndBottomZ(face);
            if (topAndBottomZ.Bottom >= viewRangeValue.DepthPlane && topAndBottomZ.Bottom < viewRangeValue.TopPlane || // bottom in the middle
                topAndBottomZ.Top > viewRangeValue.DepthPlane && topAndBottomZ.Top <= viewRangeValue.TopPlane || // top in the middle
                topAndBottomZ.Top >= viewRangeValue.TopPlane && topAndBottomZ.Bottom <= viewRangeValue.DepthPlane) // top and bottom out of the range
            {
                return true;
            }

            return false;
        }

        private List<Face> GetFaceValidOnInstance(List<FamilyInstanceInfo> listFamilyInstanceInfos, Line locationCurveOfWall)
        {
            XYZ start = locationCurveOfWall.GetEndPoint(0);
            List<Face> listFaces = new List<Face>();

            List<List<FamilyInstanceInfo>> listListIntanceInfos = GetListOfListIntanceInfos(listFamilyInstanceInfos, start);

            List<List<FamilyInstanceInfo>> mergeList = MergeLists(listListIntanceInfos);
            listFaces = GetListFacesValid(mergeList, start, locationCurveOfWall);

            return listFaces;
        }

        private List<Face> GetListFacesValid(List<List<FamilyInstanceInfo>> mergeList, XYZ start, Line locationCurveOfWall)
        {
            List<Face> listFaces = new List<Face>();

            foreach (var listInfos in mergeList)
            {
                if (listInfos.Count == 1)
                {
                    listFaces.Add(listInfos.FirstOrDefault().facePairs[0]);
                    listFaces.Add(listInfos.FirstOrDefault().facePairs[1]);
                }
                else
                {
                    listInfos.Sort((a, b) => start.DistanceTo(a.couplePoints.start).CompareTo(start.DistanceTo(b.couplePoints.start)));

                    bool result = false;
                    bool same = true;
                    for (int i = 0; i < listInfos.Count - 1; i++)
                    {
                        XYZ face1Instance1 = listInfos[0].couplePoints.start;
                        XYZ face2Instance1 = listInfos[0].couplePoints.end;
                        double distanceface1Ins1 = start.DistanceTo(face1Instance1);
                        double distanceface2Ins1 = start.DistanceTo(face2Instance1);

                        for (int j = i + 1; j < listInfos.Count; j++)
                        {
                            XYZ face1Instance2 = listInfos[j].couplePoints.start;
                            XYZ face2Instance2 = listInfos[j].couplePoints.end;

                            double distanceface1Ins2 = start.DistanceTo(face1Instance2);
                            double distanceface2Ins2 = start.DistanceTo(face2Instance2);

                            if ((distanceface1Ins2 > distanceface1Ins1 && distanceface1Ins2 < distanceface2Ins1) ||
                                (distanceface2Ins2 > distanceface1Ins1 && distanceface2Ins2 < distanceface2Ins1))
                            {
                                result = true;
                                break;
                            }
                            else if (Math.Abs(distanceface2Ins1 - distanceface1Ins2) < tolerance ||
                                   Math.Abs(distanceface1Ins1 - distanceface2Ins2) < tolerance)
                            {
                                same = false;
                            }
                        }
                    }
                    if (result == true)
                    {
                        // tim ra cạnh gần nhất và xa nhất
                        List<Face> facesInListInfos = new List<Face>();
                        foreach (var info in listInfos)
                        {
                            facesInListInfos.AddRange(info.facePairs);
                        }
                        facesInListInfos.Sort((a, b) => start.DistanceTo(ProjectPointOntoLine(GetMidPointOfFace(a), locationCurveOfWall))
                        .CompareTo(start.DistanceTo((ProjectPointOntoLine(GetMidPointOfFace(b), locationCurveOfWall)))));
                        listFaces.Add(facesInListInfos.First());
                        listFaces.Add(facesInListInfos.Last());
                    }
                    else if (result == false && same == false)
                    {
                        // tim ra 2 canh gần nhất và xa nhất, gộp các cạnh trùng
                        List<FaceInfo> listFaceInfos = new List<FaceInfo>();

                        foreach (var info in listInfos)
                        {
                            FaceInfo faceInfo1 = new FaceInfo();
                            FaceInfo faceInfo2 = new FaceInfo();
                            faceInfo1.Face = info.facePairs[0];
                            faceInfo2.Face = info.facePairs[1];
                            faceInfo1.Distance = start.DistanceTo(info.couplePoints.start);
                            faceInfo2.Distance = start.DistanceTo(info.couplePoints.end);
                            listFaceInfos.Add(faceInfo1);
                            listFaceInfos.Add(faceInfo2);
                        }

                        List<FaceInfo> listUniqueFaceInfos = GetUniquePoints(listFaceInfos);
                        listUniqueFaceInfos.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                        foreach (var faceInfo in listUniqueFaceInfos)
                        {
                            listFaces.Add(faceInfo.Face);
                        }
                    }
                    else
                    {
                        // in ra 2 canh dau va cuoi cua phan tu dau tien
                        listFaces.Add(listInfos.FirstOrDefault().facePairs[0]);
                        listFaces.Add(listInfos.FirstOrDefault().facePairs[1]);
                    }
                }
            }

            return listFaces;
        }

        private List<List<FamilyInstanceInfo>> GetListOfListIntanceInfos(List<FamilyInstanceInfo> listFamilyInstanceInfos, XYZ start)
        {
            List<List<FamilyInstanceInfo>> listListIntanceInfos = new List<List<FamilyInstanceInfo>>();
            for (int i = 0; i < listFamilyInstanceInfos.Count; i++)
            {
                List<FamilyInstanceInfo> familyInstanceInfos = new List<FamilyInstanceInfo>();
                familyInstanceInfos.Add(listFamilyInstanceInfos[i]);

                XYZ face1Instance1 = listFamilyInstanceInfos[i].couplePoints.start;
                XYZ face2Instance1 = listFamilyInstanceInfos[i].couplePoints.end;
                double distanceface1Ins1 = start.DistanceTo(face1Instance1);
                double distanceface2Ins1 = start.DistanceTo(face2Instance1);

                if (i < listFamilyInstanceInfos.Count - 1)
                {
                    for (int j = i + 1; j < listFamilyInstanceInfos.Count; j++)
                    {
                        XYZ face1Instance2 = listFamilyInstanceInfos[j].couplePoints.start;
                        XYZ face2Instance2 = listFamilyInstanceInfos[j].couplePoints.end;

                        double distanceface1Ins2 = start.DistanceTo(face1Instance2);
                        double distanceface2Ins2 = start.DistanceTo(face2Instance2);

                        if (distanceface2Ins1 >= distanceface1Ins2 &&
                            (distanceface1Ins1 <= distanceface2Ins2))
                        {
                            familyInstanceInfos.Add(listFamilyInstanceInfos[j]);
                        }
                    }
                }
                listListIntanceInfos.Add(familyInstanceInfos);
            }
            return listListIntanceInfos;
        }

        private List<FaceInfo> GetUniquePoints(List<FaceInfo> faceInfo)
        {
            return faceInfo.GroupBy(p => p.Distance)
                         .Select(g => g.First())
                         .ToList();
        }

        private List<List<FamilyInstanceInfo>> MergeLists(List<List<FamilyInstanceInfo>> lists)
        {
            List<List<FamilyInstanceInfo>> groups = new List<List<FamilyInstanceInfo>>();

            foreach (var list in lists)
            {
                List<FamilyInstanceInfo> mergedGroup = new List<FamilyInstanceInfo>(list);
                List<List<FamilyInstanceInfo>> remainingGroups = new List<List<FamilyInstanceInfo>>();

                if (groups.Count > 0)
                {
                    foreach (var group in groups)
                    {
                        if (group.Any(x => mergedGroup.Contains(x)))
                        {
                            mergedGroup.AddRange(group);
                        }
                        else
                        {
                            remainingGroups.Add(group);
                        }
                    }
                }

                remainingGroups.Add(mergedGroup.Distinct().ToList());
                groups = remainingGroups;
            }

            return groups;
        }

        private (XYZ start, XYZ end) GetProjectPointOnLocationCurve(List<Face> facePairs, Line locationCurve)
        {
            XYZ point1 = ProjectPointOntoLine(GetMidPointOfFace(facePairs[0]), locationCurve);
            XYZ point2 = ProjectPointOntoLine(GetMidPointOfFace(facePairs[1]), locationCurve);
            return (point1, point2);
        }

        private XYZ ProjectPointOntoLine(XYZ point, Line line)
        {
            IntersectionResult result = line.Project(point);

            return result != null ? result.XYZPoint : null;
        }

        private (double Top, double Bottom) GetTopAndBottomZ(Face face)
        {
            EdgeArray edges = face.EdgeLoops.get_Item(0);
            Line line = null;
            foreach (Edge edge in edges)
            {
                Line lineEdge = edge.AsCurve() as Line;
                if (lineEdge.Direction.IsAlmostEqualTo(XYZ.BasisZ, tolerance) || lineEdge.Direction.IsAlmostEqualTo(-XYZ.BasisZ, tolerance))
                {
                    line = lineEdge;
                    break;
                }
            }
            if (line.GetEndPoint(0).Z > line.GetEndPoint(1).Z)
            {
                return (line.GetEndPoint(0).Z, line.GetEndPoint(1).Z);
            }

            return (line.GetEndPoint(1).Z, line.GetEndPoint(0).Z);
        }

        private ViewRangeValue GetViewRange(ViewPlan view, double elevation)
        {
            ViewRangeValue viewRangeValue = new ViewRangeValue();

            PlanViewRange range = view.GetViewRange();
            viewRangeValue.DepthPlane = range.GetOffset(PlanViewPlane.ViewDepthPlane) + elevation;
            viewRangeValue.TopPlane = range.GetOffset(PlanViewPlane.TopClipPlane) + elevation;
            viewRangeValue.CutPlane = range.GetOffset(PlanViewPlane.CutPlane) + elevation;
            viewRangeValue.BottomPlane = range.GetOffset(PlanViewPlane.BottomClipPlane) + elevation;

            return viewRangeValue;
        }

        private List<FamilyInstanceInfo> GetListFaces(IList<Element> intersectElements, Line wallLine, XYZ start)
        {
            List<FamilyInstanceInfo> listFamilyInstanceInfos = new List<FamilyInstanceInfo>();
            foreach (Element element in intersectElements)
            {
                FamilyInstanceInfo familyInstanceInfo = new FamilyInstanceInfo();
                List<Face> facePairs = new List<Face>();
                foreach (Face face in GetFacesOnGeometry(element))
                {
                    if (face != null && IsFaceNormalWithLine(face, wallLine))
                    {
                        facePairs.Add(face);
                    }
                }
                if (facePairs.Count == 2)
                {
                    facePairs.Sort((a, b) => start.DistanceTo(ProjectPointOntoLine(GetMidPointOfFace(a), wallLine))
                           .CompareTo(start.DistanceTo((ProjectPointOntoLine(GetMidPointOfFace(b), wallLine)))));
                    familyInstanceInfo.facePairs = facePairs;
                    familyInstanceInfo.couplePoints = GetProjectPointOnLocationCurve(familyInstanceInfo.facePairs, wallLine);
                }
                listFamilyInstanceInfos.Add(familyInstanceInfo);
            }
            return listFamilyInstanceInfos;
        }

        private List<Face> GetFacesOnGeometry(Element element)
        {
            List<Face> listFaces = new List<Face>();
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            options.ComputeReferences = true;
            GeometryElement elementGeo = element.get_Geometry(options);

            foreach (GeometryObject geometryObj in elementGeo)
            {
                if (geometryObj is Solid solid)
                {
                    if (solid.Faces.Size > 0 && solid.Volume > 0)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            if (face is PlanarFace)
                            {
                                listFaces.Add(face);
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
                                foreach (Face face in nestedSolid.Faces)
                                {
                                    if (face is PlanarFace)
                                    {
                                        listFaces.Add(face);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return listFaces;
        }

        private IList<Element> ListIntersectElements(Document doc, Element wall)
        {
            ElementCategoryFilter wallFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ElementCategoryFilter columnFilter = new ElementCategoryFilter(BuiltInCategory.OST_Columns);
            LogicalOrFilter orFilter = new LogicalOrFilter(wallFilter, columnFilter);
            ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(wall);

            IList<Element> listElements = new FilteredElementCollector(doc)
                .WherePasses(orFilter)
                .WherePasses(filter)
                .ToElements();

            return listElements;
        }

        private XYZ GetOffset(XYZ point, XYZ orientation, double value)
        {
            XYZ newVector = orientation.Multiply(value);
            XYZ returnPoint = point.Add(newVector);

            return returnPoint;
        }

        private bool IsFaceNormalWithLine(Face face, Line line)
        {
            XYZ lineDirection = line.Direction;
            if (face is PlanarFace)
            {
                PlanarFace pf = face as PlanarFace;
                return pf.FaceNormal.Normalize().IsAlmostEqualTo(lineDirection.Normalize(), tolerance) ||
                    pf.FaceNormal.Normalize().IsAlmostEqualTo(-lineDirection.Normalize(), tolerance);
            }

            return false;
        }

        private List<FamilyInstance> GetInstanceOnWall(Document doc, Wall wall)
        {
            ElementCategoryFilter windowFilter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
            ElementCategoryFilter doorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            LogicalOrFilter orFilter = new LogicalOrFilter(windowFilter, doorFilter);

            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .WherePasses(orFilter)
                .OfClass(typeof(FamilyInstance));

            List<FamilyInstance> listFamilies = new List<FamilyInstance>();
            foreach (FamilyInstance instance in collector)
            {
                if (instance.Host != null && instance.Host.Id == wall.Id)
                {
                    listFamilies.Add(instance);
                }
            }
            return listFamilies;
        }

        //End________________
    }

    public class ViewRangeValue
    {
        public double TopPlane { get; set; }
        public double CutPlane { get; set; }
        public double BottomPlane { get; set; }
        public double DepthPlane { get; set; }
    }

    public class FamilyInstanceInfo
    {
        public List<Face> facePairs { get; set; }

        public (XYZ start, XYZ end) couplePoints { get; set; }
    }

    public class FaceInfo
    {
        public Face Face { get; set; }
        public double Distance { get; set; }
    }
}