using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DistributionProject
{
    [Transaction(TransactionMode.Manual)]
    public class CreateModelLineRectangle : IExternalCommand
    {
        public Result Execute(
           ExternalCommandData commandData,
           ref string message,
           ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Application app = uiApp.Application;

            ICollection<ElementId> selectedIds = uiDoc.Selection.GetElementIds();
            if (selectedIds.Count == 0)
            {
                message = "No rooms selected.";
                return Result.Failed;
            }

            List<CurveLoop> roomBoundaries = new List<CurveLoop>();

            foreach (ElementId id in selectedIds)
            {
                Element element = doc.GetElement(id);
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
                message = "No valid room boundaries found.";
                return Result.Failed;
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
            var counter = 0;
            List<Line> lastFourCreatedRectLines = null;

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
                    message = "Không tìm thấy đường line phía dưới cùng.";
                    return Result.Failed;
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
                            message = "Không tìm thấy đường line phía trên gần nhất.";
                            return Result.Failed;
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
                // Tạo hình chữ nhật
                foreach (Line rectLine in createdRectLines)
                {
                }
                bool IsCreateRectangle = CreateRectangle(doc, app, bottomLeft, bottomRight, maxHeight, out rectangleLines);
                if (IsCreateRectangle)
                {
                    foreach (var itemRectangleLines in rectangleLines)
                    {
                        createdRectLines.Add(itemRectangleLines);
                    }

                    lines = lines.Where(lineRemove => !rectangleLines.Any(rectLine => AreLinesEquivalent(rectLine, lineRemove))).ToList();
                    counter++;

                    continue;
                }
                // Loại bỏ các đường thẳng đã được bao phủ
                // Thêm các đường thẳng của hình chữ nhật vào danh sách
            }

            return Result.Succeeded;
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

        public XYZ GetMidpointY(BoundingBoxXYZ boundingBox)
        {
            // Lấy các điểm min và max của bounding box
            XYZ minPoint = boundingBox.Min;
            XYZ maxPoint = boundingBox.Max;

            // Tính trung điểm theo trục Y
            double midY = (minPoint.Y + maxPoint.Y) / 2.0;

            // Trả về trung điểm theo trục Y
            return new XYZ((minPoint.X + maxPoint.X) / 2.0, midY, (minPoint.Z + maxPoint.Z) / 2.0);
        }

        private const double Epsilon = 1e-9;

        // Method to check if a double value is almost zero within a specified tolerance
        public static bool IsAlmostZero(double value, double tolerance = Epsilon)
        {
            return Math.Abs(value) < tolerance;
        }

        // Method to check if a point is on a line
        public static bool IsPointOnLine(XYZ point, Line line)
        {
            // Calculate the cross product of the vectors (line start to point) and (line start to line end)
            XYZ lineVector = line.GetEndPoint(1) - line.GetEndPoint(0);
            XYZ pointVector = point - line.GetEndPoint(0);

            // Check if the point vector is a multiple of the line vector
            double crossProduct = lineVector.CrossProduct(pointVector).GetLength();
            if (!IsAlmostZero(crossProduct))
                return false;

            // Check if the point is within the bounds of the line segment
            double dotProduct = pointVector.DotProduct(lineVector);
            if (dotProduct < 0)
                return false;

            double squaredLength = lineVector.GetLength() * lineVector.GetLength();
            if (dotProduct > squaredLength)
                return false;

            return true;
        }

        // Method to check if one line is completely inside another line
        public static bool IsLineInsideLine(Line innerLine, Line outerLine)
        {
            // Check if both endpoints of the inner line are on the outer line
            return IsPointOnLine(innerLine.GetEndPoint(0), outerLine) && IsPointOnLine(innerLine.GetEndPoint(1), outerLine);
        }

        // Method to check if a line is inside any line in a list of lines
        public static bool IsLineInsideAnyLine(Line innerLine, List<Line> lines)
        {
            foreach (var line in lines)
            {
                if (IsLineInsideLine(innerLine, line))
                {
                    return true;
                }
            }
            return false;
        }

        private bool AreLinesEquivalent(Line line1, Line line2)
        {
            // Kiểm tra xem hai đường line có trùng nhau không
            return (line1.GetEndPoint(0).IsAlmostEqualTo(line2.GetEndPoint(0)) && line1.GetEndPoint(1).IsAlmostEqualTo(line2.GetEndPoint(1))) ||
                   (line1.GetEndPoint(0).IsAlmostEqualTo(line2.GetEndPoint(1)) && line1.GetEndPoint(1).IsAlmostEqualTo(line2.GetEndPoint(0)));
        }

        public bool IsLineInsideRectangle(Line line, Line rectLine)
        {
            XYZ start = line.GetEndPoint(0);
            XYZ end = line.GetEndPoint(1);

            // Kiểm tra xem cả hai điểm đầu mút của đường thẳng có nằm trong hình chữ nhật không
            if (IsPointInsideRectangle(start, rectLine) && IsPointInsideRectangle(end, rectLine))
            {
                return true;
            }

            return false;
        }

        public bool IsPointInsideRectangle(XYZ point, Line rectLine)
        {
            XYZ start = rectLine.GetEndPoint(0);
            XYZ end = rectLine.GetEndPoint(1);

            double minX = Math.Min(start.X, end.X);
            double maxX = Math.Max(start.X, end.X);
            double minY = Math.Min(start.Y, end.Y);
            double maxY = Math.Max(start.Y, end.Y);

            // Kiểm tra xem điểm có nằm trong phạm vi của hình chữ nhật không
            if (point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY)
            {
                return true;
            }

            return false;
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

        private Line GetBottomLine3(List<Line> lines)
        {
            // Sắp xếp theo giá trị Y nhỏ nhất trước, sau đó theo giá trị X nhỏ nhất
            List<Line> listbottomLine = lines.OrderBy(line => Math.Min(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y))
                                   .ThenBy(line => Math.Min(line.GetEndPoint(0).X, line.GetEndPoint(1).X))
                                   .ToList();
            Line bottomLine = listbottomLine.OrderBy(line => Math.Min(line.GetEndPoint(0).X, line.GetEndPoint(1).X)).FirstOrDefault();
            return bottomLine;
        }

        private List<Line> GetBottomLine2(List<Line> lines)
        {
            // Sắp xếp theo giá trị Y nhỏ nhất trước, sau đó theo giá trị X nhỏ nhất

            List<Line> listbottomLine = lines.OrderBy(line => Math.Min(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y))
                       .ThenBy(line => Math.Min(line.GetEndPoint(0).X, line.GetEndPoint(1).X))
                       .ToList();

            return listbottomLine;
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

        private Line GetNearestBottomLine(Line topLine, List<Line> lines, XYZ topLeft, XYZ topRight)
        {
            // Tìm đường line phía dưới gần nhất so với topLine
            double topY = Math.Min(topLine.GetEndPoint(0).Y, topLine.GetEndPoint(1).Y);
            List<Line> bottomLines = lines.Where(line => Math.Max(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y) < topY)
                                .OrderBy(line => Math.Max(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y))
                                .ToList();

            foreach (Line line in bottomLines)
            {
                XYZ start = line.GetEndPoint(0);
                XYZ end = line.GetEndPoint(1);

                // Kiểm tra nếu đường thẳng nằm trong khoảng từ start đến end của topLine
                if (start.X >= topLeft.X && end.X <= topRight.X)
                {
                    return line;
                }
            }
            return null;
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

        private Line GetNearestTopLine3(Line bottomLine, List<Line> lines, XYZ bottomLeft, XYZ bottomRight)
        {
            // Tìm đường line phía trên gần nhất so với bottomLine
            double bottomY = Math.Max(bottomLine.GetEndPoint(0).Y, bottomLine.GetEndPoint(1).Y);
            List<Line> topLines = lines.Where(line => Math.Min(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y) > bottomY)
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

        public Line GetNearestPerpendicularTopLine(Line bottomLine, List<Line> lines)
        {
            // Lấy giá trị Y lớn nhất của bottomLine
            double bottomMaxY = Math.Max(bottomLine.GetEndPoint(0).Y, bottomLine.GetEndPoint(1).Y);

            // Tìm tất cả các đường line phía trên bottomLine
            List<Line> topLines = lines.Where(line => Math.Min(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y) > bottomMaxY).ToList();

            // Kiểm tra tính vuông góc và tìm đường line gần nhất
            Line nearestPerpendicularLine = null;
            double minDistance = double.MaxValue;

            XYZ bottomDirection = (bottomLine.GetEndPoint(1) - bottomLine.GetEndPoint(0)).Normalize();

            foreach (var topLine in topLines)
            {
                XYZ topDirection = (topLine.GetEndPoint(1) - topLine.GetEndPoint(0)).Normalize();

                // Kiểm tra xem topLine có vuông góc với bottomLine không
                double dotProduct = bottomDirection.DotProduct(topDirection);
                if (Math.Abs(dotProduct) < 1e-6) // Kiểm tra nếu dotProduct gần bằng 0
                {
                    // Tính khoảng cách giữa bottomLine và topLine
                    double distance = Math.Min(
                        Math.Abs(topLine.GetEndPoint(0).Y - bottomMaxY),
                        Math.Abs(topLine.GetEndPoint(1).Y - bottomMaxY)
                    );

                    // Tìm đường line vuông góc gần nhất
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestPerpendicularLine = topLine;
                    }
                }
            }

            return nearestPerpendicularLine;
        }

        public List<Line> GetBottomLines(List<Line> lines, BoundingBoxXYZ boundingBox)
        {
            XYZ midpointY = GetMidpointY(boundingBox);
            // Tìm giá trị Y nhỏ nhất trong tất cả các điểm của các đường line
            double minY = lines.Min(line => Math.Min(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y));

            // Lọc ra các đường line có ít nhất một điểm nằm dưới giá trị Y nhỏ nhất
            List<Line> bottomLines = lines.Where(line => Math.Min(line.GetEndPoint(0).Y, line.GetEndPoint(1).Y) < midpointY.Y).ToList();

            return bottomLines;
        }

        private double GetMaxHeight(XYZ bottomLeft, XYZ bottomRight, List<Line> topLines)
        {
            double maxHeight = double.MaxValue;

            foreach (Line line in topLines)
            {
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
            }

            return maxHeight;
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

        private bool CreateRectangle(Document doc, Application app, XYZ bottomLeft, XYZ bottomRight, double height, out List<Line> rectangleLines)
        {
            XYZ topLeft = new XYZ(bottomLeft.X, bottomLeft.Y + height, bottomLeft.Z);
            XYZ topRight = new XYZ(bottomRight.X, bottomRight.Y + height, bottomRight.Z);

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
            bool rectangleCheck = RectangleChecker.AreLinesFormingRectangle(rectangleLines);
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

        private void CreateRectangleFromProjectedPoints(Document doc, List<XYZ> rectanglePoints)
        {
            // Tạo các đoạn thẳng nối các điểm để tạo hình chữ nhật
            List<Line> lines = new List<Line>();
            for (int i = 0; i < 4; i++)
            {
                XYZ startPoint = rectanglePoints[i];
                XYZ endPoint = rectanglePoints[(i + 1) % 4]; // Lấy điểm kế tiếp hoặc quay lại điểm đầu tiên
                Line line = Line.CreateBound(startPoint, endPoint);
                lines.Add(line);
            }

            // Tạo hình chữ nhật từ các đoạn thẳng
            CurveArray curveLoop = new CurveArray();
            foreach (Line line in lines)
            {
                curveLoop.Append(line);
            }

            // Tạo một mặt từ curve loop
            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero); // Sử dụng mặt phẳng XY
            SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
            ModelCurveArray modelCurves = doc.Create.NewModelCurveArray(curveLoop, sketchPlane);

            // Nếu cần, bạn có thể sử dụng modelCurves để thực hiện các thao tác khác, chẳng hạn như gán các thuộc tính cho đường mô hình.
        }

        public class XYZComparer : IEqualityComparer<XYZ>
        {
            public bool Equals(XYZ p1, XYZ p2)
            {
                return p1.IsAlmostEqualTo(p2);
            }

            public int GetHashCode(XYZ p)
            {
                return p.GetHashCode();
            }
        }

        // Hàm kiểm tra xem một đoạn thẳng có giao với một đa giác không
        public bool IntersectsWithPolygon(Line line, List<Line> polygon)
        {
            foreach (Line polygonLine in polygon)
            {
                if (line.Intersect(polygonLine) != SetComparisonResult.Disjoint)
                    return true;
            }
            return false;
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

        private bool AreEndpointsClose(Line line1, Line line2, double tolerance)
        {
            return line1.GetEndPoint(1).IsAlmostEqualTo(line2.GetEndPoint(0), tolerance) ||
                   line1.GetEndPoint(0).IsAlmostEqualTo(line2.GetEndPoint(1), tolerance);
        }

        private Line JoinTwoLines(Line line1, Line line2)
        {
            XYZ start = line1.GetEndPoint(0);
            XYZ end = line2.GetEndPoint(1);
            return Line.CreateBound(start, end);
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
    }
}