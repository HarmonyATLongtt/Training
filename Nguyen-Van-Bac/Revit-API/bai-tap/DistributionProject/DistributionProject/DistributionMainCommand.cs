using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace DistributionProject
{
    [Transaction(TransactionMode.Manual)]
    public class DistributionMainCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> familys = collector.OfClass(typeof(Family)).ToElements();
            string familyName = "TCエリア・矩形寸法(居室面積)";
            bool familysLoaded = familys.Any(fam => fam.Name.Equals(familyName));
            Family family = null;
            if (!familysLoaded)
            {
                string libraryPath = "";
                app.GetLibraryPaths().TryGetValue("Imperial Library", out libraryPath);
                if (string.IsNullOrEmpty(libraryPath))
                {
                    libraryPath = "c:\\";
                }

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = libraryPath;
                openFileDialog.Filter = "Family Files (*.rfa)|*.rfa";

                FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
                Level level = levelCollector.OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().Cast<Level>().First(x => x.Name == "Level 1");
                if (level != null)
                {
                    if (DialogResult.OK == openFileDialog.ShowDialog())
                    {
                        string filePathFamily = openFileDialog.FileName;
                        using (Transaction transaction = new Transaction(doc, "Load Family"))
                        {
                            transaction.Start();
                            try
                            {
                                bool load = doc.LoadFamily(filePathFamily, out family);
                                if (load && family != null)
                                {
                                    FamilySymbol familySymbol = doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;
                                    if (!familySymbol.IsActive)
                                    {
                                        familySymbol.Activate();
                                        if (familySymbol != null)
                                        {
                                            string name = filePathFamily;
                                            TaskDialog.Show("Ok", name + " đã được tải thành công");
                                            transaction.Commit();
                                            AddFamilyAndPlace(app, uidoc, doc, familySymbol, family);
                                        }
                                    }
                                }
                                else
                                {
                                    TaskDialog.Show("Error", "Load family không thành công");
                                }
                            }
                            catch (Exception ex)
                            {
                                TaskDialog.Show("Error", ex.Message);
                                return Result.Failed;
                            }
                        }
                    }
                }
            }
            else
            {
                ////thêm family đã được load vào model theo tên
                FilteredElementCollector collectorFamily = new FilteredElementCollector(doc);
                family = collectorFamily.OfClass(typeof(Family)).FirstOrDefault(f => f.Name == familyName) as Family;
                if (family == null)
                {
                    TaskDialog.Show("Error", "Không tìm thấy tên family");
                }
                FamilySymbol familySymbol = doc.GetElement(family.GetFamilySymbolIds().First()) as FamilySymbol;
                // Chèn FamilySymbol vào model

                AddFamilyAndPlace(app, uidoc, doc, familySymbol, family);
            }

            return Result.Succeeded;
        }

        public void AddFamilyAndPlace(Application app, UIDocument uidoc, Document doc, FamilySymbol familySymbol, Family family)
        {
            //Document doc2 = doc.EditFamily(family);
            IList<Reference> pickedReferences = uidoc.Selection.PickObjects(ObjectType.Element, new RoomSelectionFilter(), "Select multiple rooms");

            // Convert picked references to elements
            IList<Element> pickedRooms = new List<Element>();
            foreach (Reference reference in pickedReferences)
            {
                Element element = uidoc.Document.GetElement(reference);
                if (element != null)
                {
                    pickedRooms.Add(element);
                }
            }
            List<CurveLoop> roomBoundaries = new List<CurveLoop>();

            foreach (Element element in pickedRooms)
            {
                if (element is Room)
                {
                    Room room = element as Room;
                    // Lấy biên giới của phòng
                    IList<IList<BoundarySegment>> segments = room.GetBoundarySegments(new SpatialElementBoundaryOptions());

                    if (segments != null && segments.Count > 0)
                    {
                        foreach (IList<BoundarySegment> segmentList in segments)
                        {
                            CurveLoop curveLoop = new CurveLoop();
                            foreach (BoundarySegment seg in segmentList)
                            {
                                curveLoop.Append(seg.GetCurve());
                            }
                            roomBoundaries.Add(curveLoop);
                        }
                    }
                }
            }

            if (roomBoundaries.Count == 0)
            {
                TaskDialog.Show("Error", "No valid room boundaries found.");
            }

            // Hợp nhất các đường bao
            CurveLoop combinedBoundary = MergeCurves(roomBoundaries);
            List<Curve> projectedCurves = new List<Curve>();
            // Tạo một đường bao mới từ kết quả hợp nhất
            using (Transaction trans = new Transaction(doc, "Create Combined Boundary"))
            {
                trans.Start();

                // Chuyển đổi các curve trong combinedBoundary sang mặt phẳng XY
                SketchPlane sketchPlane = SketchPlane.Create(doc, Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero));
                Plane plane = sketchPlane.GetPlane();

                foreach (Curve curve in combinedBoundary)
                {
                    projectedCurves.Add(ProjectCurveToPlane(curve, plane));
                }
                trans.Commit();
            }
            List<Line> linesOld = new List<Line>();
            foreach (var curve in projectedCurves)
            {
                if (curve is Line line)
                {
                    linesOld.Add(line);
                }
            }
            List<Line> lines = CombineCollinearLines(linesOld);
            double MaxY = lines
            .SelectMany(line => new[] { line.GetEndPoint(0), line.GetEndPoint(1) })
            .Max(point => point.Y);
            List<Line> createdRectLines = new List<Line>();
            double centerY = MaxY / 2;
            int counCreatedRectLinesTop = 0;
            int countRectangles = 0;
            List<Line> lastFourCreatedRectLines = null;
            int indexFamilyInstances = 1;
            // Tạo hình chữ nhật từ các đường line bên dưới và chiếu lên các đường line bên trên
            while (true)
            {
                if (lines.Count == 0)
                {
                    break;
                }
                List<Line> rectangleLines = null;
                // Lấy các Line bên dưới
                Line bottomLine = GetBottomLine(lines);

                if (bottomLine == null)
                {
                    TaskDialog.Show("Error", "không tìm thấy đường line bên dưới");
                }
                Line line = bottomLine;
                XYZ start = line.GetEndPoint(0);
                XYZ end = line.GetEndPoint(1);

                // Giả sử đây là điểm góc dưới bên trái của hình chữ nhật
                XYZ bottomLeft = start.X < end.X ? start : end;
                XYZ bottomRight = start.X < end.X ? end : start;
                // Tìm đường line phía trên gần nhất
                Line topLine = GetNearestTopLine(bottomLine, lines);
                bool result = LineCheck.AreAllLinesInsideAnyLine(lines, createdRectLines);
                if (!result && bottomLine.GetEndPoint(0).Y > centerY)
                {
                    if (counCreatedRectLinesTop == 0)
                    {
                        lastFourCreatedRectLines = GetLastItems(createdRectLines, 4);
                    }

                    bottomLine = GetTopLineMax(lines);
                    start = bottomLine.GetEndPoint(0);
                    end = bottomLine.GetEndPoint(1);

                    // Giả sử đây là điểm góc dưới bên trái của hình chữ nhật
                    bottomLeft = start.X < end.X ? start : end;
                    bottomRight = start.X < end.X ? end : start;

                    topLine = GetBottomFromTopLine(bottomLine, lastFourCreatedRectLines);
                    if (topLine != null)
                    {
                        XYZ startPoint = new XYZ(bottomLeft.X, topLine.GetEndPoint(0).Y, topLine.GetEndPoint(0).Z);
                        XYZ endPoint = new XYZ(bottomRight.X, topLine.GetEndPoint(1).Y, topLine.GetEndPoint(1).Z);
                        if (startPoint.DistanceTo(endPoint) > app.AngleTolerance)
                        {
                            topLine = Line.CreateBound(startPoint, endPoint);
                            counCreatedRectLinesTop++;
                        }
                    }
                }

                double maxHeight = 0;
                if (topLine == null)
                {
                    if (lines != null)
                    {
                        double lineYMax = lines.Max(l => Math.Min(l.GetEndPoint(0).Y, l.GetEndPoint(1).Y));
                        if (MaxY == lineYMax)
                        {
                            TaskDialog.Show("Error", "Không tìm thấy đường line phía trên gần nhất.");
                        }
                    }
                }
                if (topLine != null)
                {
                    maxHeight = GetMaxHeight(bottomLeft, bottomRight, topLine);
                    // Tìm chiều cao lớn nhất có thể của hình chữ nhật
                    if (maxHeight <= 1.5 && result)
                    {
                        double bottomY = Math.Max(topLine.GetEndPoint(0).Y, topLine.GetEndPoint(1).Y);

                        Line lineTop = GetNearestTopLine2(bottomLine, lines, bottomY, bottomLeft, bottomRight);
                        if (lineTop != null)
                        {
                            maxHeight = GetMaxHeight(bottomLeft, bottomRight, lineTop);
                        }
                    }
                }

                if (maxHeight >= double.MaxValue)
                {
                    lines.Remove(line);

                    continue;
                }

                bool rectangleCheck = false;
                XYZ topLeft, topRight = null;
                bool IsCreateRectangle = CreateRectangle(doc, app, bottomLeft, bottomRight, maxHeight, out rectangleLines, out topLeft, out topRight, out rectangleCheck);
                if (IsCreateRectangle)
                {
                    if (rectangleCheck)
                    {
                        if (!familySymbol.IsActive)
                        {
                            familySymbol.Activate();
                            doc.Regenerate();
                        }
                        if (topLeft != null && bottomLeft != null && topRight != null && bottomRight != null)
                        {
                            XYZ location = GetRectangleCenter(topLeft, topRight, bottomLeft, bottomRight);
                            double width = topLeft.DistanceTo(topRight);

                            // Tính chiều cao: khoảng cách giữa topLeft và bottomLeft
                            double length = topLeft.DistanceTo(bottomLeft);
                            using (Transaction tr = new Transaction(doc, "add family"))
                            {
                                tr.Start();
                                doc.Create.NewFamilyInstance(location, familySymbol, StructuralType.NonStructural);
                                tr.Commit();
                            }
                            List<FamilyInstance> familyInstance = new FilteredElementCollector(doc)
                            .OfClass(typeof(FamilyInstance))
                            .Cast<FamilyInstance>().ToList();
                            // Lấy cái đầu tiên tìm thấy (hoặc áp dụng điều kiện lọc khác)

                            if (familyInstance.Count == 0)
                            {
                                TaskDialog.Show("Error", "No family instance found");
                            }

                            using (Transaction trans = new Transaction(doc, "Set Parameter"))
                            {
                                trans.Start();

                                // Lấy parameter cần set (sử dụng tên hoặc BuiltInParameter)
                                Parameter parameterWidth = familyInstance[countRectangles].LookupParameter("エリア・幅");
                                Parameter parameterLength = familyInstance[countRectangles].LookupParameter("エリア・長さ");
                                Parameter parameterIndex = familyInstance[countRectangles].LookupParameter("エリア名称");

                                if (parameterWidth != null && !parameterWidth.IsReadOnly && !parameterLength.IsReadOnly && parameterIndex.IsReadOnly == false)
                                {
                                    parameterLength?.Set(length);
                                    // Set giá trị cho parameter (ví dụ: giá trị double, int, string, ...)
                                    parameterWidth?.Set(width); // Hoặc parameter.Set("Some String Value")
                                    parameterIndex?.Set(indexFamilyInstances.ToString());
                                    indexFamilyInstances++;
                                }
                                else
                                {
                                    TaskDialog.Show("Error", "Parameter not found or is read - only.");
                                }

                                trans.Commit();
                            }
                        }
                        countRectangles++;
                    }
                    //var (length, width) = GetRoomDimensions(room);

                    foreach (var itemRectangleLines in rectangleLines)
                    {
                        createdRectLines.Add(itemRectangleLines);
                    }

                    lines = lines.Where(lineRemove => !rectangleLines.Any(rectLine => AreLinesEquivalent(rectLine, lineRemove))).ToList();

                    continue;
                }
            }
        }

        public static (double width, double height) CalculateDimensions(XYZ topLeft, XYZ topRight, XYZ bottomLeft, XYZ bottomRight)
        {
            // Tính chiều rộng: khoảng cách giữa topLeft và topRight
            double width = topLeft.DistanceTo(topRight);

            // Tính chiều cao: khoảng cách giữa topLeft và bottomLeft
            double height = topLeft.DistanceTo(bottomLeft);

            return (width, height);
        }

        private CurveLoop MergeCurves(List<CurveLoop> curveLoops)
        {
            Solid mergedSolid = null;

            foreach (CurveLoop loop in curveLoops)
            {
                Solid solid = CreateSolidFromCurveLoop(loop);
                if (mergedSolid == null)
                {
                    mergedSolid = solid;
                }
                else
                {
                    mergedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(mergedSolid, solid, BooleanOperationsType.Union);
                }
            }

            // Chuyển đổi solid trở lại CurveLoop
            CurveLoop mergedCurveLoop = ExtractCurveLoopFromSolid(mergedSolid);
            return mergedCurveLoop;
        }

        private Solid CreateSolidFromCurveLoop(CurveLoop loop)
        {
            List<CurveLoop> loops = new List<CurveLoop> { loop };
            SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
            Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, 0.1, options);
            return solid;
        }

        private CurveLoop ExtractCurveLoopFromSolid(Solid solid)
        {
            CurveLoop result = new CurveLoop();
            FaceArray faces = solid.Faces;
            foreach (Face face in faces)
            {
                if (face is PlanarFace)
                {
                    EdgeArray edges = face.EdgeLoops.get_Item(0);
                    foreach (Edge edge in edges)
                    {
                        Curve curve = edge.AsCurve();
                        result.Append(curve);
                    }
                    break;
                }
            }
            return result;
        }

        private Curve ProjectCurveToPlane(Curve curve, Plane plane)
        {
            XYZ normal = plane.Normal;
            XYZ origin = plane.Origin;

            Transform projectionTransform = Transform.Identity;

            // Chuyển đổi curve thành các đoạn thẳng và chiếu từng điểm lên mặt phẳng
            List<XYZ> projectedPoints = new List<XYZ>();
            foreach (XYZ point in curve.Tessellate())
            {
                XYZ vector = point - origin;
                double distance = vector.DotProduct(normal);
                XYZ projectedPoint = point - distance * normal;
                projectedPoints.Add(projectedPoint);
            }

            // Tạo lại curve từ các điểm đã được chiếu
            Curve projectedCurve = Line.CreateBound(projectedPoints[0], projectedPoints[1]);
            for (int i = 1; i < projectedPoints.Count - 1; i++)
            {
                projectedCurve = projectedCurve.CreateTransformed(Transform.Identity);
            }

            return projectedCurve;
        }

        public static List<Line> CombineCollinearLines(List<Line> lines)
        {
            var result = new List<Line>();

            while (lines.Count > 0)
            {
                var currentLine = lines[0];
                var collinearLines = new List<Line> { currentLine };

                for (int i = 1; i < lines.Count; i++)
                {
                    if (AreCollinear(currentLine, lines[i]) && AreConnected(currentLine, lines[i]))
                    {
                        collinearLines.Add(lines[i]);
                    }
                }

                var combinedLine = CombineLines(collinearLines);
                result.Add(combinedLine);

                // Loại bỏ các đoạn thẳng đã được xử lý
                lines = lines.Except(collinearLines).ToList();
            }

            return result;
        }

        public static bool AreCollinear(Line line1, Line line2)
        {
            XYZ direction1 = (line1.GetEndPoint(1) - line1.GetEndPoint(0)).Normalize();
            XYZ direction2 = (line2.GetEndPoint(1) - line2.GetEndPoint(0)).Normalize();
            return direction1.IsAlmostEqualTo(direction2);
        }

        public static bool AreConnected(Line line1, Line line2)
        {
            XYZ start1 = line1.GetEndPoint(0);
            XYZ end1 = line1.GetEndPoint(1);
            XYZ start2 = line2.GetEndPoint(0);
            XYZ end2 = line2.GetEndPoint(1);

            return start1.IsAlmostEqualTo(start2) || start1.IsAlmostEqualTo(end2) || end1.IsAlmostEqualTo(start2) || end1.IsAlmostEqualTo(end2);
        }

        public static Line CombineLines(List<Line> lines)
        {
            if (lines.Count == 1) return lines[0];

            var points = new List<XYZ>();
            foreach (var line in lines)
            {
                points.Add(line.GetEndPoint(0));
                points.Add(line.GetEndPoint(1));
            }

            // Tìm hai điểm xa nhau nhất để tạo đoạn thẳng mới
            XYZ start = points[0];
            XYZ end = points[1];
            double maxDistance = 0;

            foreach (var p1 in points)
            {
                foreach (var p2 in points)
                {
                    double distance = p1.DistanceTo(p2);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        start = p1;
                        end = p2;
                    }
                }
            }

            return Line.CreateBound(start, end);
        }

        private Line GetBottomLine(List<Line> lines)
        {
            // Sắp xếp theo giá trị Y nhỏ nhất trước, sau đó theo giá trị X nhỏ nhất
            Line bottomLine = lines.OrderBy(line => Math.Min(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y))
                                   .ThenBy(line => Math.Min(line.GetEndPoint(0).X, line.GetEndPoint(1).X))
                                   .FirstOrDefault();
            return bottomLine;
        }

        private Line GetTopLineMax(List<Line> lines)
        {
            // Sắp xếp theo giá trị X nhỏ nhất trước và lấy đường thẳng đầu tiên trong danh sách đã sắp xếp
            Line bottomLine = lines.OrderByDescending(line => Math.Max(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y))
                        .ThenBy(line => Math.Min(line.GetEndPoint(0).X, line.GetEndPoint(1).X))
                        .FirstOrDefault();
            return bottomLine;
        }

        private Line GetNearestTopLine(Line bottomLine, List<Line> lines)
        {
            // Tìm đường line phía trên gần nhất so với bottomLine
            double bottomY = Math.Max(bottomLine.GetEndPoint(0).Y, bottomLine.GetEndPoint(1).Y);
            double midX = (bottomLine.GetEndPoint(0).X + bottomLine.GetEndPoint(1).X) / 2; // Trục x ở giữ

            Line topLine = lines.Where(line => Math.Max(line.GetEndPoint(0).X, line.GetEndPoint(1).X) < midX && // Lấy các đường nằm bên trái của trục x ở giữ
                                                 Math.Max(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y) > bottomY)
                                .OrderByDescending(line => Math.Max(line.GetEndPoint(0).X, line.GetEndPoint(1).X))
                                .FirstOrDefault(); // Sắp xếp theo trục X lớn nhất và lấy đường line đầu tiên

            return topLine;
        }

        private Line GetBottomFromTopLine(Line bottomLine, List<Line> lines)
        {
            // Tìm đường line phía trên gần nhất so với bottomLine
            double bottomY = Math.Max(bottomLine.GetEndPoint(0).Y, bottomLine.GetEndPoint(1).Y);
            double midX = (bottomLine.GetEndPoint(0).X + bottomLine.GetEndPoint(1).X) / 2; // Trục x ở giữ

            Line topLine = lines.Where(line => Math.Min(line.GetEndPoint(0).X, line.GetEndPoint(1).X) < midX && // Lấy các đường nằm bên trái của trục x ở giữ
                                                 Math.Min(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y) < bottomY)
                                .OrderByDescending(line => Math.Min(line.GetEndPoint(0).X, line.GetEndPoint(1).X))
                                .FirstOrDefault(); // Sắp xếp theo trục X lớn nhất và lấy đường line đầu tiên

            return topLine;
        }

        private Line GetNearestTopLine2(Line bottomLine, List<Line> lines, double bottomYy, XYZ bottomLeft, XYZ bottomRight)
        {
            // Tìm đường line phía trên gần nhất so với bottomLine
            double bottomY = Math.Max(bottomLine.GetEndPoint(0).Y, bottomLine.GetEndPoint(1).Y);
            List<Line> topLines = lines.Where(line => Math.Min(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y) > bottomYy)
                                .OrderBy(line => Math.Min(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y))
                                .ToList();

            foreach (Line line in topLines)
            {
                XYZ start = line.GetEndPoint(0);
                XYZ end = line.GetEndPoint(1);

                // Kiểm tra nếu đường thẳng cắt ngang từ bottomLeft đến bottomRight
                if (start.X <= bottomRight.X && end.X >= bottomLeft.X)
                {
                    return line;
                }
            }
            return null;
        }

        private double GetMaxHeight(XYZ bottomLeft, XYZ bottomRight, Line line)
        {
            double maxHeight = double.MaxValue;

            XYZ start = line.GetEndPoint(0);
            XYZ end = line.GetEndPoint(1);

            // Kiểm tra nếu đường thẳng cắt ngang từ bottomLeft đến bottomRight
            if (start.X <= bottomRight.X && end.X >= bottomLeft.X)
            {
                // Tính khoảng cách từ bottom đến line
                double height = start.Y - bottomLeft.Y;
                if (height < maxHeight)
                {
                    maxHeight = height;
                }
            }

            return maxHeight;
        }

        private bool CreateRectangle(Document doc, Application app, XYZ bottomLeft, XYZ bottomRight, double height, out List<Line> rectangleLines, out XYZ topLeft, out XYZ topRight, out bool rectangleCheck)
        {
            topLeft = new XYZ(bottomLeft.X, bottomLeft.Y + height, bottomLeft.Z);
            topRight = new XYZ(bottomRight.X, bottomRight.Y + height, bottomRight.Z);

            List<XYZ> points = new List<XYZ> { bottomLeft, bottomRight, topRight, topLeft };

            // Tạo các đoạn thẳng nối các điểm để tạo hình chữ nhật
            rectangleLines = new List<Line>();
            for (int i = 0; i < 4; i++)
            {
                XYZ startPoint = points[i];
                XYZ endPoint = points[(i + 1) % 4]; // Lấy điểm kế tiếp hoặc quay lại điểm đầu tiên
                if (startPoint.DistanceTo(endPoint) > app.AngleTolerance)
                {
                    Line line = Line.CreateBound(startPoint, endPoint);
                    rectangleLines.Add(line);
                }
            }

            rectangleCheck = RectangleChecker.AreLinesFormingRectangle(rectangleLines);
            if (rectangleCheck)
            {
                CurveArray curveLoop = new CurveArray();
                foreach (Line line in rectangleLines)
                {
                    curveLoop.Append(line);
                }

                // Tạo một mặt từ curve loop
                using (Transaction t = new Transaction(doc, "Create Rectangle"))
                {
                    t.Start();
                    Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, bottomLeft); // Sử dụng mặt phẳng XY
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                    foreach (Line line in rectangleLines)
                    {
                        doc.Create.NewModelCurve(line, sketchPlane);
                    }
                    t.Commit();
                }
                return true;
            }
            return true;
        }

        public static List<T> GetLastItems<T>(List<T> list, int count)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");
            }

            int startIndex = Math.Max(0, list.Count - count);
            return list.GetRange(startIndex, list.Count - startIndex);
        }

        private bool AreLinesEquivalent(Line line1, Line line2)
        {
            // Kiểm tra xem hai đường line có trùng nhau không
            return (line1.GetEndPoint(0).IsAlmostEqualTo(line2.GetEndPoint(0)) && line1.GetEndPoint(1).IsAlmostEqualTo(line2.GetEndPoint(1))) ||
                   (line1.GetEndPoint(0).IsAlmostEqualTo(line2.GetEndPoint(1)) && line1.GetEndPoint(1).IsAlmostEqualTo(line2.GetEndPoint(0)));
        }

        public static XYZ GetRectangleCenter(XYZ topLeft, XYZ topRight, XYZ bottomLeft, XYZ bottomRight)
        {
            double centerX = (topLeft.X + topRight.X + bottomLeft.X + bottomRight.X) / 4;
            double centerY = (topLeft.Y + topRight.Y + bottomLeft.Y + bottomRight.Y) / 4;
            double centerZ = (topLeft.Z + topRight.Z + bottomLeft.Z + bottomRight.Z) / 4;

            return new XYZ(centerX, centerY, centerZ);
        }
    }

    public class RoomSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is Room;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }

    public class FamilyInstanceWithLocation
    {
        public FamilyInstance FamilyInstance { get; set; }
        public XYZ Location { get; set; }
    }
}