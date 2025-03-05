using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;
using System.Xml.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using FirstCommand.View;

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

            Reference pickedRef = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Chọn một bức tường");

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
                        WallType wallType = selectedWall.WallType;

                        if (wallType != null)
                        {
                            listLines = CutWall(doc, selectedWall);

                            using (Transaction trans = new Transaction(doc, "Divide a wall"))
                            {
                                //trans.Start();
                                //foreach (Element ele in GetElementIds(doc, selectedWall))
                                //{
                                //    doc.Delete(ele.Id);
                                //}
                                //trans.Commit();

                                //trans.Start();

                                if (listLines == null)
                                {
                                    return Result.Failed;
                                }
                                foreach (Line line in listLines)
                                {
                                    trans.Start();
                                    Wall newWall = CreateWall(doc, line, wallTypeId, selectedWall);
                                    trans.Commit();

                                    if (selectedWall.WallType.Kind == WallKind.Curtain)
                                    {
                                        CopyCurtainGrid(line, selectedWall, newWall);
                                        CopyMullions(selectedWall, newWall);
                                    }
                                }
                                trans.Start();
                                doc.Delete(selectedWall.Id);
                                trans.Commit();

                                //trans.Commit();
                            }

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

        private IList<Element> GetElementIds(Document doc, Wall selectedWall)
        {
            IList<Element> intersectElements = new List<Element>();
            try
            {
                intersectElements = ListIntersectElements(doc, selectedWall);
            }
            catch
            {
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
            }

            return intersectElements;
        }

        private IList<Element> ListIntersectElements(Document doc, Element wall)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(wall);
            return collector.WherePasses(filter).ToElements();
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
                List<object> listParaOrigins = GetContraintAndOffSet(selectedWall, doc);
                List<object> listParaIntersects = GetContraintAndOffSet(intersectElement, doc);

                if (listParaOrigins.SequenceEqual(listParaIntersects))
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

            BoundingBoxXYZ boundingBox = element.get_BoundingBox(null);
            Solid bboxSolid = CreateSolidFromBoundingBox(boundingBox);

            foreach (Face face in bboxSolid.Faces)
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

            //foreach (GeometryObject geometryObject in elementGeometry)
            //{
            //    if (geometryObject is Solid solid)
            //    {
            //        //SolidCurveIntersectionOptions intersectOptions = new SolidCurveIntersectionOptions();

            //        //SolidCurveIntersection intersection = solid.IntersectWithCurve(curve, intersectOptions);

            //        //if (intersection != null && intersection.SegmentCount == 0)
            //        //{
            //        //    TaskDialog.Show("Error", "Không tìm thấy điểm giao cắt!");
            //        //    return null;
            //        //}

            //        //for (int i = 0; i < intersection.SegmentCount; i++)
            //        //{
            //        //    Curve intersectCurve = intersection.GetCurveSegment(i);
            //        //    XYZ firstCutPoint = intersectCurve.GetEndPoint(0);
            //        //    XYZ lastCutPoint = intersectCurve.GetEndPoint(1);
            //        //    intersectPoints.Add(firstCutPoint);
            //        //    intersectPoints.Add(lastCutPoint);
            //        //}

            //        foreach (Face face in solid.Faces)
            //        {
            //            IntersectionResultArray results;
            //            SetComparisonResult comparisonResult = face.Intersect(curve, out results);

            //            if (comparisonResult == SetComparisonResult.Overlap && results != null)
            //            {
            //                foreach (IntersectionResult result in results)
            //                {
            //                    intersectPoints.Add(result.XYZPoint);
            //                }
            //            }
            //        }
            //    }
            //}
            if (intersectPoints.Count != 2)
            {
                //TaskDialog.Show("Error", "Cutting wall does not intersect the original wall at two points.");
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

                using (Transaction trans = new Transaction(doc, "Add Grid Line"))
                {
                    trans.Start();
                    try
                    {
                        newWall.CurtainGrid.AddGridLine(isUGridLine, position, false);
                    }
                    catch (Exception)
                    {
                        //TaskDialog.Show("Error", "Lỗi khi thêm Grid Line: " + ex.Message);
                    }
                    trans.Commit();
                }
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
            foreach (List<LineInfo> listLineInfos in listIntersecPoints)
            {
                CurtainGridLine gridLine = doc.GetElement(listLineInfos.FirstOrDefault().GridLineId) as CurtainGridLine;

                foreach (LineInfo lineInfo in listLineInfos)
                {
                    using (Transaction trans = new Transaction(doc, "Add Mullion"))
                    {
                        trans.Start();

                        try
                        {
                            gridLine.AddMullions(lineInfo.Curve, lineInfo.Type, true);
                        }
                        catch (Exception)
                        {
                            //TaskDialog.Show("Error", "Lỗi khi thêm Grid Line: " + ex.Message);
                        }
                        trans.Commit();
                    }
                }
            }
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
}