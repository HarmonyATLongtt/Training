using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

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
    }
}