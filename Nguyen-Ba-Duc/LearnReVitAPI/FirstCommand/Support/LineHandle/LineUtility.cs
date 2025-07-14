using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using FirstCommand.Support.Constants;
using FirstCommand.Support.PlaneHandle;
using FirstCommand.Support.GeometryHandle;
using FirstCommand.Support.VectorHandle;

namespace FirstCommand.Support.LineHandle
{
    public static class LineUtility
    {
        /// <summary>
        /// Chiếu 2 điểm của 1 line lên 1 mặt phẳng, mục đích để tạo ra 1 line mới nằm trên plane
        /// sử dụng trong trường hợp line cũ nằm rất gần plane nhưng do sai số nên không nằm trên plane đó
        /// </summary>
        /// <param name="line"></param>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static Line ProjectLineOntoSketchPlane(Line line, Plane plane)
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
        /// Hàm tạo line nếu khoảng cách các điểm là phù hợp
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        public static void CreateLineFromTriangleEdges(XYZ p1, XYZ p2, List<Line> lines)
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
        /// Hàm chiếu 1 line lên 1 plane và tạo ra line mới
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Line CreateLineOnPlane(Plane plane, Line line)
        {
            XYZ p1 = line.GetEndPoint(0);
            XYZ p2 = line.GetEndPoint(1);
            XYZ newPoint1 = PlaneUtility.GetProjectedPoint(plane, p1);
            XYZ newPoint2 = PlaneUtility.GetProjectedPoint(plane, p2);

            Line newLine = Line.CreateBound(newPoint1, newPoint2);
            return newLine;
        }

        /// <summary>
        /// Đưa các cặp line về cùng 1 mặt phẳng để xét giao cắt, chỉ xét 2 trường hợp là line 2 vuông góc hoặc song song với mặt phẳng
        /// chứa line 1, mặt phẳng chứa line1 là mặt phẳng song song với trục Z
        /// </summary>
        /// <param name="lines"></param>
        public static void MakeLinesCoplanar(List<Line> lines)
        {
            // Đưa các cặp line về cùng 1 mặt phẳng để tìm giao điểm
            for (int i = 0; i < lines.Count - 1; i++)
            {
                XYZ dir1 = lines[i].Direction.Normalize();
                XYZ dir2 = lines[i + 1].Direction.Normalize();
                // Tạo mặt phẳng đi qua 1 line và thẳng đứng song song với trục Z
                Plane plane = PlaneUtility.CreatePlaneParallelToZFromLine(lines[i]);
                // Nếu line[i + 1] song song với plane
                if (VectorUtility.ArePerpendicular(plane.Normal.Normalize(), dir2, CommonConstants.COSINE_ANGLE_TOLERANCE_1_DEGREE))
                {
                    lines[i + 1] = CreateLineOnPlane(plane, lines[i + 1]);
                    // Sau đó sẽ xét giao điểm
                }
                // Nếu line[i + 1] vuông góc với plane
                else if (VectorUtility.AreParallel(plane.Normal.Normalize(), dir2, CommonConstants.COSINE_ANGLE_TOLERANCE_1_DEGREE))
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
        }

        /// <summary>
        /// Hàm lấy ra giao điểm của 2 line bound
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static XYZ GetIntersectionPoint(Line line1, Line line2)
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

        /// <summary>
        /// Kiểm tra xem 2 đường có trùng nhau không
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static bool AreLinesColinear(Line line1, Line line2)
        {
            // Vector hướng
            XYZ dir1 = (line1.GetEndPoint(1) - line1.GetEndPoint(0)).Normalize();
            XYZ dir2 = (line2.GetEndPoint(1) - line2.GetEndPoint(0)).Normalize();

            // Kiểm tra song song (cross product gần 0 vector)
            XYZ cross = dir1.CrossProduct(dir2);
            bool areParallel = cross.GetLength() < CommonConstants.TOLERANCE;

            if (!areParallel)
                return false;

            // Kiểm tra cùng phương (vector nối 2 gốc nằm trên cùng đường thẳng)
            XYZ vectorBetween = line2.GetEndPoint(0) - line1.GetEndPoint(0);
            XYZ cross2 = vectorBetween.CrossProduct(dir1);
            return cross2.GetLength() < CommonConstants.TOLERANCE;
        }

        /// <summary>
        /// Kiểm tra xem 2 line có vuông góc với nhau không với 1 sai số
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static bool AreLinesPerpendicular(Line line1, Line line2)
        {
            XYZ dir1 = (line1.GetEndPoint(1) - line1.GetEndPoint(0)).Normalize();
            XYZ dir2 = (line2.GetEndPoint(1) - line2.GetEndPoint(0)).Normalize();

            double dot = dir1.DotProduct(dir2);
            return Math.Abs(dot) < CommonConstants.COSINE_ANGLE_TOLERANCE_1_DEGREE;
        }

        /// <summary>
        /// Tạo 1 Bound line từ 1 điểm và 1 vector và chiều dài của line
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static Line CreateLine(XYZ origin, XYZ direction, double length)
        {
            // Nên giới han chiều dài của Line tránh trường hợp 2 line vuông góc với nhau, và do quá dài nên cắt nhau, gây ra sai điểm giao
            //double length = 10;
            //XYZ midPoint = SetOriginPoint(origin, (origin.Z + topZ) / 2);
            XYZ p1 = origin + direction.Normalize().Multiply(-length);
            XYZ p2 = origin + direction.Normalize().Multiply(length);

            return Line.CreateBound(p1, p2);
        }

        /// <summary>
        /// Hàm tìm điểm thuộc một đường thẳng (Line) là hình chiếu vuông góc từ một điểm khác
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        //public static XYZ GetPerpendicularProjectionPointOnLine(Line line, XYZ point)
        //{
        //    if (line == null || point == null || !line.IsBound)
        //        throw new ArgumentException("Invalid input");

        //    // Điểm đầu và cuối của line
        //    XYZ p0 = line.GetEndPoint(0);
        //    XYZ p1 = line.GetEndPoint(1);

        //    // Vector chỉ phương của đường thẳng
        //    XYZ lineDirection = (p1 - p0).Normalize();

        //    // Vector từ điểm gốc của line đến điểm cần chiếu
        //    XYZ vectorToPoint = point - p0;

        //    // Chiều dài chiếu của vector lên đường thẳng (tức là khoảng cách theo hướng line)
        //    double projectionLength = vectorToPoint.DotProduct(lineDirection);

        //    // Tọa độ điểm chiếu vuông góc trên line
        //    XYZ projectedPoint = p0 + projectionLength * lineDirection;

        //    return projectedPoint;
        //}

        public static XYZ GetPerpendicularProjectionPointOnLine(Line line, XYZ point)
        {
            if (line == null || point == null)
                throw new ArgumentNullException("Line or point is null");

            XYZ origin;
            XYZ direction;

            if (line.IsBound)
            {
                // Nếu là bound line → lấy điểm đầu làm gốc
                origin = line.GetEndPoint(0);
                direction = (line.GetEndPoint(1) - line.GetEndPoint(0)).Normalize();
            }
            else
            {
                // Nếu là unbound line → lấy origin và direction trực tiếp
                origin = line.Origin;
                direction = line.Direction;
            }

            // Vector từ origin đến điểm cần chiếu
            XYZ vectorToPoint = point - origin;

            // Độ dài chiếu của vector lên đường thẳng
            double projectionLength = vectorToPoint.DotProduct(direction);

            // Tọa độ điểm chiếu vuông góc
            XYZ projectedPoint = origin + projectionLength * direction;

            return projectedPoint;
        }

        /// <summary>
        /// Hàm tính khoảng cách từ 1 point đến 1 line
        /// </summary>
        /// <param name="point"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static double DistancePointToLine(XYZ point, Line line)
        {
            XYZ lineOrigin = line.GetEndPoint(0);
            XYZ lineDirection = (line.GetEndPoint(1) - lineOrigin).Normalize();
            XYZ vectorToPoint = point - lineOrigin;

            XYZ cross = vectorToPoint.CrossProduct(lineDirection);
            double distance = cross.GetLength();

            return distance;
        }

        /// <summary>
        /// Hàm dùng để tạo 1 mặt phẳng từ 2 line không song song với nhau và đi qua 1 line
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static Plane CreatePlaneFromTwoLineAndIncludeOneLine(Line line1, Line line2, double tolerance)
        {
            Plane plane = null;
            XYZ dir1 = line1.Direction.Normalize();
            XYZ dir2 = line2.Direction.Normalize();
            XYZ origin = line1.GetEndPoint(0);
            if (!VectorUtility.AreParallel(dir1, dir2, tolerance))
            {
                var cross = dir1.CrossProduct(dir2);
                plane = Plane.CreateByNormalAndOrigin(cross, origin);
            }
            return plane;
        }

        /// <summary>
        /// Hàm lấy ra giao điểm của 2 line nếu kéo dài ra
        /// </summary>
        /// <param name="line1"></param>
        /// <param name="line2"></param>
        /// <returns></returns>
        public static XYZ FindIntersectionFromLines(Line line1, Line line2)
        {
            XYZ dir1 = (line1.GetEndPoint(1) - line1.GetEndPoint(0)).Normalize();
            XYZ dir2 = (line2.GetEndPoint(1) - line2.GetEndPoint(0)).Normalize();

            Line unbound1 = Line.CreateUnbound(line1.GetEndPoint(0), dir1);
            Line unbound2 = Line.CreateUnbound(line2.GetEndPoint(0), dir2);

            SetComparisonResult result = unbound1.Intersect(unbound2, out IntersectionResultArray resultArray);

            if (result == SetComparisonResult.Overlap && resultArray != null && resultArray.Size > 0)
            {
                return resultArray.get_Item(0).XYZPoint;
            }

            return null;
        }

        /// <summary>
        /// Hàm này dùng để kiểm tra xem 1 điểm nằm giữa 2 điểm đầu và cuối của 1 Line hay nằm ngoài
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsPointBetweenTwoPointsOfLine(Line line, XYZ point)
        {
            double lineLength = line.Length;
            XYZ p1 = line.GetEndPoint(0);
            XYZ p2 = line.GetEndPoint(1);
            double dis1 = point.DistanceTo(p1);
            double dis2 = point.DistanceTo(p2);

            if (dis1 > lineLength || dis2 > lineLength)
            {
                return false;
            }
            return true;
        }
    }
}