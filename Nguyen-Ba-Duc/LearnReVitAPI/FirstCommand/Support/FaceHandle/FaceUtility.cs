using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FirstCommand.Support.Constants;

namespace FirstCommand.Support.FaceHandle
{
    public static class FaceUtility
    {
        /// <summary>
        /// Hàm kiểm tra xem face có vuông góc với 1 trục tọa độ hay không
        /// </summary>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static bool IsFacePerpendicularToAxis(XYZ normal, XYZ vectorAxis)
        {
            double dot = normal.DotProduct(vectorAxis);
            return Math.Abs(Math.Abs(dot) - 1) < CommonConstants.TOLERANCE;
        }

        /// <summary>
        /// Lấy bán kính của 1 CylindricalFace
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public static double GetRadius(CylindricalFace face)
        {
            CylindricalSurface s = face.GetSurface() as CylindricalSurface;
            double radius = s.Radius;
            return radius;
        }

        /// <summary>
        ///  Hàm kiểm tra xem face có song song với trục tọa độ hay không
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool IsFaceParallelToAxis(XYZ normal, XYZ axis)
        {
            //return Math.Abs(normal.DotProduct(XYZ.BasisZ)) < tolerance;
            //return Math.Abs(normal.DotProduct(XYZ.BasisZ)) < 0.01;
            return Math.Abs(normal.DotProduct(axis)) < CommonConstants.COSINE_ANGLE_TOLERANCE_1_DEGREE;
        }

        /// <summary>
        /// Hàm lấy ra danh sách các vertices của 1 face
        /// </summary>
        /// <param name="planarFace"></param>
        /// <returns></returns>
        public static IList<XYZ> GetVerticesOfFace(Face face)
        {
            IList<XYZ> vertices = new List<XYZ>();
            Mesh mesh = face.Triangulate();
            if (mesh == null || mesh.Vertices.Count == 0)
            {
                TaskDialog.Show("Noti", "Face không có mesh hoặc không có đỉnh");
            }
            else
            {
                vertices = mesh.Vertices;
            }
            return vertices;
        }

        /// <summary>
        /// Kiểm tra xem 1 line có giao với 1 face hay không
        /// </summary>
        /// <param name="line"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        public static XYZ GetIntersectionPoint(Line line, Face face)
        {
            IntersectionResultArray results;
            SetComparisonResult result = face.Intersect(line, out results);

            if (result == SetComparisonResult.Overlap && results != null && results.Size > 0)
            {
                return results.get_Item(0).XYZPoint;
            }
            return null;
        }

        /// <summary>
        /// Kiểm tra xem 2 PlanarFace có song song với nhau hay không
        /// </summary>
        /// <param name="face1"></param>
        /// <param name="face2"></param>
        /// <returns>Trả về true nếu song song và false nếu không</returns>
        public static bool AreFacesParallel(PlanarFace face1, PlanarFace face2)
        {
            XYZ normal1 = face1.FaceNormal.Normalize();
            XYZ normal2 = face2.FaceNormal.Normalize();
            XYZ cross = normal1.CrossProduct(normal2);

            return cross.GetLength() < CommonConstants.TOLERANCE;
        }

        /// <summary>
        /// Lấy ra PlanarFace có diện tích nhỏ nhất trong solid
        /// </summary>
        /// <param name="solid">Tham số truyền vào là 1 solid</param>
        /// <returns>Trả về 1 face là face có diện tích nhỏ nhất</returns>
        public static PlanarFace GetSmallestFace(Solid solid)
        {
            PlanarFace smallestFace = null;
            double minArea = double.MaxValue;
            foreach (Face face in solid.Faces)
            {
                if (face is PlanarFace planarFace)
                {
                    double area = planarFace.Area;
                    if (area < minArea)
                    {
                        minArea = area;
                        smallestFace = planarFace;
                    }
                }
            }
            return smallestFace;
        }

        /// <summary>
        /// Lấy ra tâm của 1 face
        /// </summary>
        /// <param name="face"></param>
        /// <returns>Trả về 1 điểm XYZ là tâm của 1 face</returns>
        public static XYZ GetCenterOfFace(Face face)
        {
            BoundingBoxUV bbox = face.GetBoundingBox();
            UV centerUV = (bbox.Min + bbox.Max) * 0.5;
            XYZ centerXYZ = face.Evaluate(centerUV);
            return centerXYZ;
        }

        /// <summary>
        /// Lấy ra danh sách và số lượng các cạnh của 1 face
        /// </summary>
        /// <param name="face"></param>
        /// <returns>Trả về 1 tuple với giả trị đầu tiên là danh sách các cạnh, giá trị thứ 2 là số lượng</returns>
        public static (List<Edge> Edges, int Num) GetEgdesAndNumOfFace(PlanarFace face)
        {
            EdgeArrayArray edgeArrays = face.EdgeLoops;
            List<Edge> listEdge = new List<Edge>();
            int sum = 0;
            foreach (EdgeArray edges in edgeArrays)
            {
                foreach (Edge edge in edges)
                {
                    listEdge.Add(edge);
                }
                sum += edges.Size;
            }

            return (listEdge, sum);
        }
    }
}