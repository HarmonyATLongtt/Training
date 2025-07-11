using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Autodesk.Revit.DB;

using FirstCommand.Support.GenericClass;
using FirstCommand.Support.GenericClass.ComparerUtils;
using FirstCommand.Support.DebugTest;
using FirstCommand.Support.PointHandle;

namespace FirstCommand.Support.SolidHandle
{
    public static class SolidUtility
    {
        /// <summary>
        /// Hàm dùng để kiểm tra xem line có giao cắt với solid hay không
        /// </summary>
        /// <param name="line"></param>
        /// <param name="solid"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsLineIntersectSolid(Line line, Solid solid)
        {
            if (line == null || solid == null)
                throw new ArgumentNullException("Line or solid is null");

            // Dùng phương thức Intersect với Line (Curve) và Solid
            SolidCurveIntersection intersection = solid.IntersectWithCurve(line, new SolidCurveIntersectionOptions());

            // Kết quả có ít nhất một điểm giao là giao cắt
            return intersection.SegmentCount > 0;
        }

        /// <summary>
        /// Hàm này dùng để lấy ra danh sách các planarface và cylindricalface từ 1 solid
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="solid"></param>
        /// <returns>Trả về 1 tuple chứa danh sách planarface và cylindricalface </returns>
        public static (List<PlanarFace>, List<CylindricalFace>) GetGroupedFacesFromSolid(Document doc, Solid solid)
        {
            List<PlanarFace> planarFaces = new List<PlanarFace>();
            List<CylindricalFace> cylindricalFaces = new List<CylindricalFace>();

            if (solid.Faces.Size > 0 && solid.Volume > 0)
            {
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
            }
            return (planarFaces, cylindricalFaces);
        }

        /// <summary>
        /// Lấy ra các point của 1 solid
        /// </summary>
        /// <param name="solid"></param>
        public static List<XYZ> GetPointOnSolid(Solid solid)
        {
            HashSet<XYZ> points = new HashSet<XYZ>(Comparers.XYZ);

            foreach (Face face in solid.Faces)
            {
                IList<CurveLoop> loops = face.GetEdgesAsCurveLoops();

                foreach (CurveLoop loop in loops)
                {
                    foreach (Curve curve in loop)
                    {
                        points.Add(curve.GetEndPoint(0));
                        points.Add(curve.GetEndPoint(1));
                    }
                }
            }
            return points.ToList();
        }

        /// <summary>
        /// Hàm tạo solid từ danh sách các points, axis, height
        /// </summary>
        /// <param name="result"></param>
        /// <param name="axis"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Solid CreateNewSolidFromPoints(List<List<(XYZ, XYZ)>> result, XYZ axis, double height)
        {
            if (result.Count == 1)
            {
                CurveLoop loop = new CurveLoop();
                foreach (var (start, end) in result.First())
                {
                    loop.Append(Line.CreateBound(start, end));
                }
                // Tạo hình khối có lỗ
                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(
                    new List<CurveLoop> { loop },
                    axis, // hướng đùn
                    height      // chiều cao
                );
                return solid;
            }
            else if (result.Count == 2)
            {
                CurveLoop outerLoop = new CurveLoop();
                CurveLoop innerLoop = new CurveLoop();
                CurveLoop loop1 = new CurveLoop();
                CurveLoop loop2 = new CurveLoop();
                double length1 = 0;
                foreach (var (start, end) in result.First())
                {
                    Line line = Line.CreateBound(start, end);
                    length1 += line.Length;
                    loop1.Append(line);
                }

                // CurveLoop cho lỗ trong

                double length2 = 0;
                foreach (var (start, end) in result.Last())
                {
                    Line line = Line.CreateBound(start, end);
                    length2 += line.Length;
                    loop2.Append(line);
                }

                if (length1 > length2)
                {
                    outerLoop = loop1;
                    innerLoop = loop2;
                }
                else
                {
                    outerLoop = loop2;
                    innerLoop = loop1;
                }
                if (!outerLoop.IsCounterclockwise(XYZ.BasisZ))
                {
                    outerLoop = CurveLoop.Create(outerLoop.Reverse().ToList());
                }
                if (innerLoop.IsCounterclockwise(XYZ.BasisZ))
                {
                    innerLoop = CurveLoop.Create(innerLoop.Reverse().ToList());
                }

                // Tạo hình khối có lỗ
                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(
                    new List<CurveLoop> { outerLoop, innerLoop }, // outer + inner
                    axis, // hướng đùn
                    height     // chiều cao
                );
                return solid;
            }
            else { return null; }
        }

        /// <summary>
        /// Tạo solid hình trụ
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="height"></param>
        /// <param name="extrusionDirection"></param>
        /// <returns></returns>
        public static Solid CreateCylindricalSolid(Document doc, XYZ center, double radius, double height, XYZ extrusionDirection)
        {
            // 1.Tạo 2 vector UV để tạo mặt phẳng phác thảo
            PointUtility.BuildLocalUVFromDirection(extrusionDirection, out XYZ U, out XYZ V);

            // 2. Tạo đường tròn trên mặt phẳng phác thảo
            Arc arc1 = Arc.Create(center, radius, 0, Math.PI, U, V);
            Arc arc2 = Arc.Create(center, radius, Math.PI, 2 * Math.PI, U, V);

            CurveLoop profile = new CurveLoop();
            profile.Append(arc1);
            profile.Append(arc2);

            // 3. Hướng extrusion (đảm bảo là vector đơn vị)
            extrusionDirection = extrusionDirection.Normalize();

            // 4. Tạo hình trụ bằng extrusion
            Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(
                new List<CurveLoop> { profile }, extrusionDirection, height
            );

            return solid;
        }
    }
}