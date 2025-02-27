using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xaml;
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
        // Note: sử lý trường hợp cột không đồng phẳng, lấy ra face ở mặt ngoài cùng
        // Sử lý trường hợp tường basic giao với stacked wall
        // Sử lý trường hợp curtain wall
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
                                trans.Start();
                                foreach (Element ele in GetElementIds(doc, selectedWall))
                                {
                                    doc.Delete(ele.Id);
                                }
                                trans.Commit();

                                trans.Start();

                                if (listLines == null)
                                {
                                    return Result.Failed;
                                }
                                foreach (Line line in listLines)
                                {
                                    CreateWall(doc, line, wallTypeId, selectedWall);
                                }

                                doc.Delete(selectedWall.Id);

                                trans.Commit();
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

        private Wall CreateWall(Document doc, Line line, ElementId wallTypeId, Wall selectedWall)
        {
            Wall wall = Wall.Create(doc, line, wallTypeId, selectedWall.LevelId
                                   , selectedWall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble()
                                   , selectedWall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble(), false, false);
            return wall;
        }

        private IList<Element> GetElementIds(Document doc, Wall selectedWall)
        {
            IList<Element> intersectElements = new List<Element>();
            try
            {
                intersectElements = listIntersectElements(doc, selectedWall);
            }
            catch
            {
                IList<ElementId> stackedWallIds = selectedWall.GetStackedWallMemberIds();

                if (stackedWallIds != null)
                {
                    foreach (ElementId wallId in stackedWallIds)
                    {
                        Element wall = doc.GetElement(wallId);
                        intersectElements = listIntersectElements(doc, wall);
                        break;
                    }
                }
            }

            return intersectElements;
        }

        private IList<Element> listIntersectElements(Document doc, Element wall)
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
                    intersectElements = listIntersectElements(doc, selectedWall);
                    break;

                case WallKind.Stacked:
                    IList<ElementId> stackedWallIds = selectedWall.GetStackedWallMemberIds();

                    if (stackedWallIds != null)
                    {
                        foreach (ElementId wallId in stackedWallIds)
                        {
                            Element wall = doc.GetElement(wallId);
                            intersectElements = listIntersectElements(doc, wall);

                            break;
                        }
                    }
                    break;

                case WallKind.Curtain:
                    break;

                default:
                    TaskDialog.Show("Notice", "Unknown wall");
                    break;
            }

            intersectElements.Remove(selectedWall);

            LocationCurve originalWallLocationCurve = selectedWall.Location as LocationCurve;
            Curve originalCurve = originalWallLocationCurve.Curve;

            BoundingBoxXYZ bboxOrigin = selectedWall.get_BoundingBox(null);

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
                if (GetIntersectPoints(originalCurve, intersectElement) != null)
                {
                    listLines.Add(GetIntersectPoints(originalCurve, intersectElement));
                }
            }

            if (listLines.Count() <= 1)
            {
                return CreateLineByPoints(listLines, originalCurve);
            }

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
            return CreateLineByPoints(mergedLines, originalCurve);
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

        private Line GetIntersectPoints(Curve curve, Element element)
        {
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement elementGeometry = element.get_Geometry(options);
            List<XYZ> intersectPoints = new List<XYZ>();

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
    }
}