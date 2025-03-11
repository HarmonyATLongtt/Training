using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using FirstCommand.View;
using static System.Net.Mime.MediaTypeNames;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class DivideWallCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            Reference pickedRef = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick a wall");

            if (pickedRef != null)
            {
                Element element = doc.GetElement(pickedRef);
                if (element is Wall wall)
                {
                    Wall selectedWall = wall as Wall;
                    ElementId wallTypeId = selectedWall.GetTypeId();
                    List<Line> listLines = new List<Line>();
                    if (selectedWall != null)
                    {
                        List<FamilyInfo> listInfos = GetInstanceOnWall(doc, selectedWall);
                        ////GetDimOnWall(doc, selectedWall);

                        WallType wallType = selectedWall.WallType;

                        if (wallType != null)
                        {
                            listLines = CutWall(doc, selectedWall);

                            if (listLines == null)
                            {
                                return Result.Failed;
                            }
                            foreach (Line line in listLines)
                            {
                                ElementId newWallId = null;
                                RunTransaction(doc, "Divide a wall", (Transaction t) =>
                                {
                                    Wall newWall = CreateWall(doc, line, wallTypeId, selectedWall);
                                    newWallId = newWall.Id;
                                    ListInfosOnNewWall(line, listInfos);
                                    PlaceFamilyOnWall(doc, newWall, ListInfosOnNewWall(line, listInfos));
                                });
                                if (selectedWall.WallType.Kind == WallKind.Curtain)
                                {
                                    Wall newWall = doc.GetElement(newWallId) as Wall;

                                    CopyCurtainGrid(line, selectedWall, newWall);
                                    CopyMullions(selectedWall, newWall);
                                    CopyPanel(selectedWall, newWall);
                                }
                            }

                            RunTransaction(doc, "Delete a wall", (Transaction t) =>
                            {
                                doc.Delete(selectedWall.Id);
                            });

                            return Result.Succeeded;
                        }
                        return Result.Failed;
                    }
                    return Result.Failed;
                }
                TaskDialog.Show("Note", "Element has been selected is not a wall");
                return Result.Failed;
            }
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

        //______________
        //Start--Get Familes On Wall

        private List<FamilyInfo> GetInstanceOnWall(Document doc, Wall wall)
        {
            ElementCategoryFilter windowFilter = new ElementCategoryFilter(BuiltInCategory.OST_Windows);
            ElementCategoryFilter doorFilter = new ElementCategoryFilter(BuiltInCategory.OST_Doors);
            LogicalOrFilter orFilter = new LogicalOrFilter(windowFilter, doorFilter);

            FilteredElementCollector collector = new FilteredElementCollector(doc)
                .WherePasses(orFilter)
                .OfClass(typeof(FamilyInstance));

            ElementId wallId = wall.Id;
            IList<ElementId> listWallIds = new List<ElementId>();
            if (wall.WallType.Kind == WallKind.Stacked)
            {
                listWallIds = wall.GetStackedWallMemberIds();
            }

            List<FamilyInfo> listInfos = new List<FamilyInfo>();
            foreach (FamilyInstance instance in collector)
            {
                if (instance.Host != null && (instance.Host.Id == wall.Id || listWallIds.Contains(instance.Host.Id)))
                {
                    FamilyInfo info = new FamilyInfo();

                    info.familyInstance = instance;
                    info.location = ((LocationPoint)instance.Location).Point; ;
                    info.handFlipped = instance.HandFlipped;
                    info.facingFlipped = instance.FacingFlipped;
                    info.facingOrientation = instance.FacingOrientation;

                    listInfos.Add(info);
                }
            }
            return listInfos;
        }

        private List<FamilyInfo> ListInfosOnNewWall(Line line, List<FamilyInfo> listInfos)
        {
            List<FamilyInfo> list = new List<FamilyInfo>();
            XYZ start = line.GetEndPoint(0);
            XYZ end = line.GetEndPoint(1);
            foreach (FamilyInfo info in listInfos)
            {
                if (info.location.X >= start.X && info.location.X <= end.X)
                {
                    list.Add(info);
                }
            }
            return list;
        }

        private void PlaceFamilyOnWall(Document doc, Wall newWall, List<FamilyInfo> listInfos)
        {
            Level level = doc.GetElement(newWall.LevelId) as Level;
            //using (Transaction t = new Transaction(doc))
            //{
            //    t.Start("Create new family instance");

            foreach (FamilyInfo info in listInfos)
            {
                FamilySymbol symbol = info.familyInstance.Symbol;

                FamilyInstance newFamilyInstance = doc.Create.NewFamilyInstance(info.location, symbol, newWall, level, StructuralType.NonStructural);

                newFamilyInstance.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM)
                    .Set(info.familyInstance.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM).AsDouble());

                if (info.handFlipped != newFamilyInstance.HandFlipped)
                {
                    newFamilyInstance.flipHand();
                }
                if (info.facingFlipped != newFamilyInstance.FacingFlipped)
                {
                    newFamilyInstance.flipFacing();
                }

                //if (!newWindow.FacingOrientation.IsAlmostEqualTo(info.facingOrientation))
                //{
                //    newWindow.flipFacing();
                //}
            }
            //    t.Commit();
            //}
        }

        private List<Dimension> GetDimOnWall(Document doc, Wall oldWall)
        {
            List<Dimension> dimensions = new FilteredElementCollector(doc)
                .OfClass(typeof(Dimension))
                .Cast<Dimension>()
                .Where(d => d.References.Cast<Reference>().Any(r => r.ElementId == oldWall.Id))
                .ToList();
            foreach (Dimension dim in dimensions)
            {
                ReferenceArray references = dim.References;
                List<ElementId> elementIds = new List<ElementId>();

                foreach (Reference reference in references)
                {
                    ElementId id = reference.ElementId;
                    if (id != ElementId.InvalidElementId)
                    {
                        elementIds.Add(id);
                    }
                }
            }

            return dimensions;
        }

        //______________
        //Start--CutWall
        private Wall CreateWall(Document doc, Line line, ElementId wallTypeId, Wall selectedWall)
        {
            Wall wall = Wall.Create(doc, line, wallTypeId, selectedWall.LevelId
                                   , selectedWall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble()
                                   , selectedWall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble(), false, false);

            WallUtils.DisallowWallJoinAtEnd(wall, 0); // Điểm đầu
            WallUtils.DisallowWallJoinAtEnd(wall, 1); // Điểm cuối

            return wall;
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

        private List<object> GetContraintAndOffSet(Element element, Document doc)
        {
            List<object> listObjects = new List<object>();
            if (element is Wall wall)
            {
                ElementId baseConstraintId = wall.get_Parameter(BuiltInParameter.WALL_BASE_CONSTRAINT).AsElementId();
                Level baseLevel = doc.GetElement(baseConstraintId) as Level;
                ElementId topConstraintId = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).AsElementId();
                Level topLevel = doc.GetElement(topConstraintId) as Level;
                double baseOffset = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();
                double topOffset = wall.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsDouble();
                listObjects.Add(baseLevel.Id);
                listObjects.Add(topLevel.Id);
                listObjects.Add(baseOffset);
                listObjects.Add(topOffset);
            }
            else if (element is FamilyInstance column
                      && (column.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Columns
                      || column.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralColumns))
            {
                ElementId baseConstraintId = column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM).AsElementId();
                Level baseLevel = doc.GetElement(baseConstraintId) as Level;
                ElementId topConstraintId = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).AsElementId();
                Level topLevel = doc.GetElement(topConstraintId) as Level;
                double baseOffset = column.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM).AsDouble();
                double topOffset = column.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                listObjects.Add(baseLevel.Id);
                listObjects.Add(topLevel.Id);
                listObjects.Add(baseOffset);
                listObjects.Add(topOffset);
            }

            return listObjects;
        }

        private List<Line> CutWall(Document doc, Wall selectedWall)
        {
            IList<Element> intersectElements = new List<Element>();

            switch (selectedWall.WallType.Kind)
            {
                case WallKind.Basic:
                    intersectElements = ListIntersectElements(doc, selectedWall);
                    break;

                case WallKind.Stacked:

                    IList<ElementId> stackedWallIds = selectedWall.GetStackedWallMemberIds();

                    if (stackedWallIds != null)
                    {
                        foreach (ElementId wallId in stackedWallIds)
                        {
                            Element wall = doc.GetElement(wallId);
                            intersectElements = ListIntersectElements(doc, wall);

                            break;
                        }
                    }
                    break;

                case WallKind.Curtain:

                    intersectElements = ListIntersectElements(doc, selectedWall);
                    break;

                default:
                    TaskDialog.Show("Notice", "Unknown wall");
                    break;
            }

            intersectElements.Remove(selectedWall);

            LocationCurve originalWallLocationCurve = selectedWall.Location as LocationCurve;

            Curve originalCurve = originalWallLocationCurve.Curve;

            List<Line> listLines = new List<Line>();

            foreach (Element intersectElement in intersectElements)
            {
                //List<object> listParaOrigins = GetContraintAndOffSet(selectedWall, doc);
                //List<object> listParaIntersects = GetContraintAndOffSet(intersectElement, doc);

                //if (listParaOrigins.SequenceEqual(listParaIntersects))
                //{
                //    if (GetIntersectPoints(originalCurve, intersectElement) != null)
                //    {
                //        listLines.Add(GetIntersectPoints(originalCurve, intersectElement));
                //    }
                //}
                if (IsInterect(intersectElement, selectedWall))
                {
                    if (GetIntersectPoints(originalCurve, intersectElement) != null)
                    {
                        listLines.Add(GetIntersectPoints(originalCurve, intersectElement));
                    }
                }
            }

            if (listLines.Count() <= 1)
            {
                return CreateLineByPoints(listLines, originalCurve);
            }

            return CreateLineByPoints(ListMergeLines(listLines), originalCurve);
        }

        // Xem có giao với đầu trên và dưới hay không
        private bool IsInterect(Element element, Wall wall)

        {
            BoundingBoxXYZ bboxWall = wall.get_BoundingBox(null);
            BoundingBoxXYZ bboxElement = element.get_BoundingBox(null);

            if (bboxElement.Min.Z <= bboxWall.Min.Z && bboxElement.Max.Z >= bboxWall.Max.Z)
            {
                return true;
            }
            return false;
        }

        private List<Line> ListMergeLines(List<Line> listLines)
        {
            List<Line> mergedLines = new List<Line>();
            Line mergedLine = null;

            for (int i = 0; i < listLines.Count; i++)
            {
                if (mergedLine == null)
                {
                    mergedLine = listLines[i];
                }
                else
                {
                    Line nextLine = listLines[i];
                    Line tempMergedLine = MergeLines(mergedLine, nextLine);

                    if (tempMergedLine != null)
                    {
                        mergedLine = tempMergedLine;
                    }
                    else
                    {
                        mergedLines.Add(mergedLine);
                        mergedLine = nextLine;
                    }
                }
            }

            if (mergedLine != null)
            {
                mergedLines.Add(mergedLine);
            }
            return mergedLines;
        }

        private List<Line> CreateLineByPoints(List<Line> listLines, Curve originalCurve)
        {
            List<XYZ> points = new List<XYZ>();
            points.Add(originalCurve.GetEndPoint(0));
            points.Add(originalCurve.GetEndPoint(1));
            foreach (Line line in listLines)
            {
                XYZ start = line.GetEndPoint(0);
                XYZ end = line.GetEndPoint(1);
                points.Add(start);
                points.Add(end);
            }
            points.Sort((p1, p2) => originalCurve.GetEndPoint(0).DistanceTo(p1).CompareTo(originalCurve.GetEndPoint(0).DistanceTo(p2)));

            List<Line> lines = new List<Line>();
            Dictionary<XYZ, XYZ> couplePoints = new Dictionary<XYZ, XYZ>();
            for (int i = 0; i < points.Count() - 1; i += 2)
            {
                couplePoints.Add(points[i], points[i + 1]);
            }
            foreach (var couple in couplePoints)
            {
                Line line = Line.CreateBound(couple.Key, couple.Value);
                lines.Add(line);
            }

            return lines;
        }

        private Solid CreateSolidFromBoundingBox(BoundingBoxXYZ bbox)
        {
            // Lấy min và max từ BoundingBox
            XYZ min = bbox.Min;
            XYZ max = bbox.Max;

            double height = max.Z - min.Z;

            // Tạo đáy là một hình chữ nhật theo mặt phẳng XY
            CurveLoop baseLoop = CreateRectangleLoop(
                new XYZ(min.X, min.Y, min.Z),
                new XYZ(max.X, min.Y, min.Z),
                new XYZ(max.X, max.Y, min.Z),
                new XYZ(min.X, max.Y, min.Z)
            );

            // Kiểm tra tính hợp lệ của CurveLoop trước khi tạo Solid
            if (!baseLoop.HasPlane())
            {
                TaskDialog.Show("Warning", "CurveLoop is not coplanar. Cannot create solid.");
            }

            Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { baseLoop }, XYZ.BasisZ, height);

            return solid;
        }

        private CurveLoop CreateRectangleLoop(XYZ p1, XYZ p2, XYZ p3, XYZ p4)
        {
            CurveLoop loop = new CurveLoop();
            loop.Append(Line.CreateBound(p1, p2));
            loop.Append(Line.CreateBound(p2, p3));
            loop.Append(Line.CreateBound(p3, p4));
            loop.Append(Line.CreateBound(p4, p1));
            return loop;
        }

        private Line GetIntersectPoints(Curve curve, Element element)
        {
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement elementGeometry = element.get_Geometry(options);
            List<XYZ> intersectPoints = new List<XYZ>();

            //BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
            //Solid bboxSolid = CreateSolidFromBoundingBox(boundingBox);

            //foreach (Face face in bboxSolid.Faces)
            //{
            //    IntersectionResultArray results;
            //    SetComparisonResult comparisonResult = face.Intersect(curve, out results);

            //    if (comparisonResult == SetComparisonResult.Overlap && results != null)
            //    {
            //        foreach (IntersectionResult result in results)
            //        {
            //            intersectPoints.Add(result.XYZPoint);
            //        }
            //    }
            //}

            foreach (GeometryObject geometryObject in elementGeometry)
            {
                if (geometryObject is Solid solid)
                {
                    //SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();

                    //SolidCurveIntersection intersection = solid.IntersectWithCurve(curve, intersectOptions);

                    //if (intersection != null && intersection.SegmentCount == 0)
                    //{
                    //    TaskDialog.Show("Error", "Không tìm thấy điểm giao cắt!");
                    //    return null;
                    //}

                    //for (int i = 0; i < intersection.SegmentCount; i++)
                    //{
                    //    Curve intersectCurve = intersection.GetCurveSegment(i);
                    //    XYZ firstCutPoint = intersectCurve.GetEndPoint(0);
                    //    XYZ lastCutPoint = intersectCurve.GetEndPoint(1);
                    //    intersectPoints.Add(firstCutPoint);
                    //    intersectPoints.Add(lastCutPoint);
                    //}
                    foreach (Face face in solid.Faces)
                    {
                        IntersectionResultArray results;
                        SetComparisonResult comparisonResult = face.Intersect(curve, out results);

                        if (comparisonResult == SetComparisonResult.Overlap && results != null)
                        {
                            foreach (IntersectionResult result in results)
                            {
                                intersectPoints.Add(result.XYZPoint);
                            }
                        }
                    }
                }
            }
            if (intersectPoints.Count != 2)
            {
                return null;
            }
            Line line = Line.CreateBound(intersectPoints[0], intersectPoints[1]);

            return line;
        }

        private Line MergeLines(Line line1, Line line2)
        {
            XYZ startPoint1 = line1.GetEndPoint(0);
            XYZ endPoint1 = line1.GetEndPoint(1);

            XYZ startPoint2 = line2.GetEndPoint(0);
            XYZ endPoint2 = line2.GetEndPoint(1);

            XYZ direction1 = line1.Direction;
            XYZ direction2 = line2.Direction;

            if (!direction1.Normalize().IsAlmostEqualTo(direction2.Normalize(), 1e-9)
                && !direction1.Normalize().IsAlmostEqualTo(-direction2.Normalize(), 1e-9))
            {
                return null; // Hai đoạn thẳng không cùng hướng
            }
            if (line1.Project(startPoint2).XYZPoint.IsAlmostEqualTo(startPoint2, 1e-9))
            {
                return Line.CreateBound(startPoint1, endPoint2);
            }
            else if (line1.Project(endPoint2).XYZPoint.IsAlmostEqualTo(endPoint2, 1e-9))
            {
                return Line.CreateBound(startPoint2, endPoint1);
            }
            else if (line2.Project(endPoint1).XYZPoint.IsAlmostEqualTo(endPoint1, 1e-9))
            {
                return Line.CreateBound(startPoint2, endPoint2);
            }

            return null;
        }

        // End_____________________

        //_________________________
        // Start--Add CurtainGridLine
        private void CopyCurtainGrid(Line line, Wall originWall, Wall newWall)
        {
            CurtainGrid originalGrid = originWall.CurtainGrid;

            Document doc = originWall.Document;

            ICollection<ElementId> uGridLines = originalGrid.GetUGridLineIds();
            ICollection<ElementId> vGridLines = originalGrid.GetVGridLineIds();

            bool isUGridLine;
            // Sao chép lưới dọc (V direction)
            foreach (ElementId vGridLineId in vGridLines)
            {
                isUGridLine = false;
                AddGridLines(line, isUGridLine, vGridLineId, newWall, doc);
            }
            // Sao chép lưới ngang (U direction)
            foreach (ElementId uGridLineId in uGridLines)
            {
                isUGridLine = true;
                AddGridLines(line, isUGridLine, uGridLineId, newWall, doc);
            }
        }

        private void AddGridLines(Line line, bool isUGridLine, ElementId elementId, Wall newWall, Document doc)
        {
            CurtainGridLine gridLine = doc.GetElement(elementId) as CurtainGridLine;
            if (gridLine != null)
            {
                Curve curve = gridLine.FullCurve;

                XYZ position = curve.Project(line.GetEndPoint(0)).XYZPoint;

                //XYZ position = curve.Evaluate(0.1, true);

                RunTransaction(doc, "Add Grid Line", (Transaction t) =>
                {
                    try
                    {
                        newWall.CurtainGrid.AddGridLine(isUGridLine, position, false);
                    }
                    catch (Exception)
                    {
                        //TaskDialog.Show("Error", "Lỗi khi thêm Grid Line: " + ex.Message);
                    }
                });

                //using (Transaction trans = new Transaction(doc, "Add Grid Line"))
                //{
                //    trans.Start();
                //    try
                //    {
                //        newWall.CurtainGrid.AddGridLine(isUGridLine, position, false);
                //    }
                //    catch (Exception)
                //    {
                //        //TaskDialog.Show("Error", "Lỗi khi thêm Grid Line: " + ex.Message);
                //    }
                //    trans.Commit();
                //}
            }
        }

        // End________________

        //______________
        //Start--Add Mullions

        private List<MullionInfo> ListMullionInfos(Document doc, CurtainGrid cutainGrid)
        {
            List<MullionInfo> listMullionInfos = new List<MullionInfo>();
            foreach (ElementId mullionId in cutainGrid.GetMullionIds())
            {
                Mullion originMullion = doc.GetElement(mullionId) as Mullion;

                if (originMullion != null)
                {
                    XYZ mullionLocation = (originMullion.Location as LocationPoint).Point;
                    MullionInfo mullionInfo = new MullionInfo();
                    mullionInfo.LocationCurve = originMullion.LocationCurve;
                    mullionInfo.Type = originMullion.MullionType;
                    listMullionInfos.Add(mullionInfo);
                }
            }

            return listMullionInfos;
        }

        private void CopyMullions(Wall originWall, Wall newWall)
        {
            CurtainGrid originalGrid = originWall.CurtainGrid;
            Document doc = originWall.Document;

            List<MullionInfo> listMullionInfos = ListMullionInfos(doc, originalGrid);

            List<List<LineInfo>> listIntersecPoints = IntersecPoints(doc, newWall);

            foreach (MullionInfo mullionInfo in listMullionInfos)
            {
                foreach (List<LineInfo> listLineInfos in listIntersecPoints)
                {
                    foreach (LineInfo lineInfo in listLineInfos)
                    {
                        if (mullionInfo.InCurve(lineInfo.GetPoint()))
                        {
                            lineInfo.Type = mullionInfo.Type;
                        }
                    }
                }
            }

            RunTransaction(doc, "Add Mullion", (Transaction t) =>
            {
                foreach (List<LineInfo> listLineInfos in listIntersecPoints)
                {
                    CurtainGridLine gridLine = doc.GetElement(listLineInfos.FirstOrDefault().GridLineId) as CurtainGridLine;

                    foreach (LineInfo lineInfo in listLineInfos)
                    {
                        try
                        {
                            gridLine.AddMullions(lineInfo.Curve, lineInfo.Type, true);
                        }
                        catch (Exception)
                        {
                            //TaskDialog.Show("Error", "Lỗi khi thêm Grid Line: " + ex.Message);
                        }
                    }
                }
            });

            //using (Transaction trans = new Transaction(doc, "Add Mullion"))
            //{
            //    trans.Start();
            //    foreach (List<LineInfo> listLineInfos in listIntersecPoints)
            //    {
            //        CurtainGridLine gridLine = doc.GetElement(listLineInfos.FirstOrDefault().GridLineId) as CurtainGridLine;

            //        foreach (LineInfo lineInfo in listLineInfos)
            //        {
            //            //using (Transaction trans = new Transaction(doc, "Add Mullion"))
            //            //{
            //            //    trans.Start();

            //            try
            //            {
            //                gridLine.AddMullions(lineInfo.Curve, lineInfo.Type, true);
            //            }
            //            catch (Exception)
            //            {
            //                //TaskDialog.Show("Error", "Lỗi khi thêm Grid Line: " + ex.Message);
            //            }
            //            //    trans.Commit();
            //            //}
            //        }
            //    }
            //    trans.Commit();
            //}
        }

        private List<List<LineInfo>> IntersecPoints(Document doc, Wall wall)
        {
            ICollection<ElementId> uGridLines = wall.CurtainGrid.GetUGridLineIds();
            ICollection<ElementId> vGridLines = wall.CurtainGrid.GetVGridLineIds();

            List<XYZ> intersectPoints = new List<XYZ>();
            foreach (ElementId uId in uGridLines)
            {
                CurtainGridLine uGridLine = doc.GetElement(uId) as CurtainGridLine;

                foreach (ElementId vId in vGridLines)
                {
                    CurtainGridLine vGridLine = doc.GetElement(vId) as CurtainGridLine;

                    IntersectionResultArray results;
                    SetComparisonResult comparisonResult = uGridLine.FullCurve.Intersect(vGridLine.FullCurve, out results);
                    if (comparisonResult == SetComparisonResult.Overlap && results != null)
                    {
                        foreach (IntersectionResult result in results)
                        {
                            intersectPoints.Add(result.XYZPoint);
                        }
                    }
                }
            }

            List<List<LineInfo>> listLineInfos = new List<List<LineInfo>>();

            listLineInfos = ListLineInfos(doc, vGridLines, intersectPoints);
            listLineInfos.AddRange(ListLineInfos(doc, uGridLines, intersectPoints));

            return listLineInfos;
        }

        private List<List<LineInfo>> ListLineInfos(Document doc, ICollection<ElementId> GridLines, List<XYZ> intersectPoints)
        {
            List<List<LineInfo>> listLineInfos = new List<List<LineInfo>>();
            foreach (ElementId id in GridLines)
            {
                CurtainGridLine gridLine = doc.GetElement(id) as CurtainGridLine;
                Curve curve = gridLine.FullCurve as Curve;
                List<XYZ> listPoints = new List<XYZ>();
                listPoints.Add(curve.GetEndPoint(0));
                listPoints.Add(curve.GetEndPoint(1));
                foreach (XYZ intersect in intersectPoints)
                {
                    if (curve.Distance(intersect) <= 0.01)
                    {
                        listPoints.Add(intersect);
                    }
                }
                List<LineInfo> lineInfos = new List<LineInfo>();
                listPoints.Sort((p1, p2) => curve.GetEndPoint(0).DistanceTo(p1).CompareTo(curve.GetEndPoint(0).DistanceTo(p2)));

                for (int i = 0; i < listPoints.Count() - 1; i++)
                {
                    LineInfo lineInfo = new LineInfo();
                    lineInfo.SetCurve(listPoints[i], listPoints[i + 1]);
                    lineInfo.GridLineId = id;
                    lineInfos.Add(lineInfo);
                }
                listLineInfos.Add(lineInfos);
            }
            return listLineInfos;
        }

        //End____________

        //________________
        //Start-- Add Panel
        private void CopyPanel(Wall oldWall, Wall newWall)
        {
            Document doc = oldWall.Document;

            List<Panel> listOldPanels = new List<Panel>();
            foreach (ElementId panelId in oldWall.CurtainGrid.GetPanelIds())
            {
                Panel oldPanel = doc.GetElement(panelId) as Panel;
                if (oldPanel.Name != "Glazed")
                {
                    listOldPanels.Add(oldPanel);
                }
            }

            RunTransaction(doc, "Add Panel", (Transaction t) =>
            {
                ICollection<ElementId> listPanelIds = newWall.CurtainGrid.GetPanelIds();
                foreach (ElementId elementId in newWall.CurtainGrid.GetPanelIds())
                {
                    Panel newPanel = doc.GetElement(elementId) as Panel;

                    foreach (Panel oldPanel in listOldPanels)
                    {
                        if (IsBoundingBoxInside(oldPanel, newPanel))
                        {
                            newPanel.PanelType = oldPanel.PanelType;
                        }
                    }
                }
            });
        }

        private bool IsBoundingBoxInside(Panel oldPanel, Panel newPanel)
        {
            BoundingBoxXYZ oldBox = oldPanel.get_BoundingBox(null);
            BoundingBoxXYZ newBox = newPanel.get_BoundingBox(null);

            if (oldBox == null || newBox == null)
            {
                return false;
            }
            XYZ midNew = (newBox.Min + newBox.Max) / 2;
            XYZ oldMin = oldBox.Min;
            XYZ oldMax = oldBox.Max;

            bool inside = midNew.X >= oldMin.X && midNew.X <= oldMax.X &&
                            midNew.Y >= oldMin.Y && midNew.Y <= oldMax.Y &&
                            midNew.Z >= oldMin.Z && midNew.Z <= oldMax.Z;

            return inside;
        }

        //End_____________
    }

    public class MullionInfo
    {
        public MullionType Type { get; set; }
        public Curve LocationCurve { get; set; }

        public bool InCurve(XYZ point)
        {
            return LocationCurve.Distance(point) <= 0.0001;
        }
    }

    public class LineInfo
    {
        public MullionType Type { get; set; }
        public Curve Curve { get; set; }

        public ElementId GridLineId { get; set; }

        public void SetCurve(XYZ start, XYZ end)
        {
            Line line = Line.CreateBound(start, end);

            Curve = line as Curve;
        }

        public XYZ GetPoint()
        {
            if (Curve != null)
            {
                return Curve.Evaluate(0.5, true);
            }
            return null;
        }
    }

    public class FamilyInfo
    {
        public FamilyInstance familyInstance { get; set; }

        public XYZ location { get; set; }

        public bool handFlipped { get; set; }

        public bool facingFlipped { get; set; }

        public XYZ facingOrientation { get; set; }
    }

    public class WarningSuppressor : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();

            foreach (FailureMessageAccessor failure in failureMessages)
            {
                if (failure.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(failure); // Xóa cảnh báo
                }
            }

            return FailureProcessingResult.Continue;
        }
    }
}