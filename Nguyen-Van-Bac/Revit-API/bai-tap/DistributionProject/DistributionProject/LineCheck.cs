using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionProject
{
    public class LineCheck
    {
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

        public static bool AreAllLinesInsideAnyLine(List<Line> list1, List<Line> list2)
        {
            foreach (var innerLine in list1)
            {
                if (!IsLineInsideAnyLine(innerLine, list2))
                {
                    return false;
                }
            }
            return true;
        }

        // Method to check if any line in list1 is inside any line in list2
        public static List<Line> CheckLinesInLists(List<Line> list1, List<Line> list2)
        {
            List<Line> result = new List<Line>();
            foreach (var innerLine in list1)
            {
                if (IsLineInsideAnyLine(innerLine, list2))
                {
                    result.Add(innerLine);
                }
            }
            return result;
        }
    }
}