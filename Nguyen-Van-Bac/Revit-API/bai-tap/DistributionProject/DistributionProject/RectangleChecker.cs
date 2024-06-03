using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributionProject
{
    using System;
    using System.Collections.Generic;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    public class RectangleChecker
    {
        public static bool AreLinesFormingRectangle(List<Line> lines)
        {
            if (lines == null || lines.Count != 4)
            {
                return false;
            }

            // Bước 1: Kiểm tra các đường thẳng có nằm trên cùng một mặt phẳng không
            XYZ normal1 = (lines[0].GetEndPoint(1) - lines[0].GetEndPoint(0)).CrossProduct(lines[1].GetEndPoint(1) - lines[1].GetEndPoint(0));
            XYZ normal2 = (lines[2].GetEndPoint(1) - lines[2].GetEndPoint(0)).CrossProduct(lines[3].GetEndPoint(1) - lines[3].GetEndPoint(0));

            if (!normal1.IsAlmostEqualTo(normal2))
            {
                return false;
            }

            // Bước 2 và 3: Kiểm tra các cạnh đối song song, các cạnh kề vuông góc, và các điểm cuối khớp nhau
            for (int i = 0; i < lines.Count; i++)
            {
                Line line1 = lines[i];
                Line line2 = lines[(i + 1) % 4];

                XYZ dir1 = (line1.GetEndPoint(1) - line1.GetEndPoint(0)).Normalize();
                XYZ dir2 = (line2.GetEndPoint(1) - line2.GetEndPoint(0)).Normalize();

                // Kiểm tra các cạnh kề vuông góc
                if (!IsPerpendicular(dir1, dir2))
                {
                    return false;
                }

                // Kiểm tra các điểm cuối khớp nhau
                if (!line1.GetEndPoint(1).IsAlmostEqualTo(line2.GetEndPoint(0)))
                {
                    return false;
                }
            }

            // Kiểm tra các cạnh đối song song
            if (!IsParallel((lines[0].GetEndPoint(1) - lines[0].GetEndPoint(0)).Normalize(),
                            (lines[2].GetEndPoint(1) - lines[2].GetEndPoint(0)).Normalize()) ||
                !IsParallel((lines[1].GetEndPoint(1) - lines[1].GetEndPoint(0)).Normalize(),
                            (lines[3].GetEndPoint(1) - lines[3].GetEndPoint(0)).Normalize()))
            {
                return false;
            }

            return true;
        }

        private static bool IsParallel(XYZ dir1, XYZ dir2)
        {
            return dir1.CrossProduct(dir2).IsAlmostEqualTo(XYZ.Zero);
        }

        private static bool IsPerpendicular(XYZ dir1, XYZ dir2)
        {
            return dir1.DotProduct(dir2).IsAlmostEqualTo(0);
        }
    }

    public static class XYZExtensions
    {
        private const double Tolerance = 1e-6;

        public static bool IsAlmostEqualTo(this XYZ point1, XYZ point2)
        {
            return point1.DistanceTo(point2) < Tolerance;
        }

        public static bool IsAlmostEqualTo(this double value1, double value2)
        {
            return Math.Abs(value1 - value2) < Tolerance;
        }
    }
}