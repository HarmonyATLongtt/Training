using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using FirstCommand.Support.Constants;
using FirstCommand.Support.VectorHandle;

namespace FirstCommand.Support.TrianglesHandle
{
    /// <summary>
    /// Tất cả thông tin cần lấy của 1 tam giác
    /// </summary>
    public class TriangleInfos : IEquatable<TriangleInfos>
    {
        public MeshTriangle MeshTriangle { get; }
        public XYZ P1 { get; }
        public XYZ P2 { get; }
        public XYZ P3 { get; }
        public XYZ Normal { get; }

        public double MinX { get; private set; }
        public double MinY { get; private set; }
        public double MinZ { get; private set; }
        public double MaxX { get; private set; }
        public double MaxY { get; private set; }
        public double MaxZ { get; private set; }
        public XYZ MaxZPoint { get; private set; }
        public XYZ MinZPoint { get; private set; }

        public TriangleInfos(MeshTriangle meshTriangle)
        {
            MeshTriangle = meshTriangle;
            P1 = meshTriangle.get_Vertex(0);
            P2 = meshTriangle.get_Vertex(1);
            P3 = meshTriangle.get_Vertex(2);
            XYZ vectorNomal = ComputeNormal(P1, P2, P3);
            if (vectorNomal != null)
            {
                Normal = vectorNomal;
            }
            GetMinMaxXYZOfTriangle();
            GetHighestAndLowestZPoint();
        }

        /// <summary>
        /// Hàm lấy ra điểm thấp nhất và cao nhất của 1 tam giác
        /// </summary>
        /// <param name="triangle"></param>
        /// <returns></returns>
        private void GetHighestAndLowestZPoint()
        {
            XYZ highest = P1;
            XYZ lowest = P1;

            if (P2.Z > highest.Z) highest = P2;
            if (P2.Z < lowest.Z) lowest = P2;

            if (P3.Z > highest.Z) highest = P3;
            if (P3.Z < lowest.Z) lowest = P3;

            MaxZPoint = highest;
            MinZPoint = lowest;
        }

        /// <summary>
        /// Tính vector normal của tam giác
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private XYZ ComputeNormal(XYZ a, XYZ b, XYZ c, double tolerance = CommonConstants.TOLERANCE)
        {
            if ((b - a).GetLength() < tolerance ||
                (c - a).GetLength() < tolerance ||
                (c - b).GetLength() < tolerance)
            {
                // Tam giác suy biến hoặc cạnh quá ngắn
                return null; // hoặc XYZ.Zero, hoặc throw exception
            }

            var v1 = b - a;
            var v2 = c - a;
            var cross = v1.CrossProduct(v2);
            if (cross.GetLength() < tolerance)
                return null;

            return cross.Normalize();
            //return VectorUtility.NormalizeSafe(cross);
        }

        /// <summary>
        /// Lấy ra danh sách 3 đỉnh của tam giác
        /// </summary>
        /// <returns></returns>
        public List<XYZ> Vertexes()
        {
            return new List<XYZ> { P1, P2, P3 };
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TriangleInfos);
        }

        public bool Equals(TriangleInfos other)
        {
            if (other == null) return false;
            return MeshTriangle.Equals(other.MeshTriangle);
        }

        public override int GetHashCode()
        {
            return MeshTriangle.GetHashCode();
        }

        /// <summary>
        /// Hàm lấy ra giá trị lớn nhất của X,Y,Z trong tam giác
        /// </summary>
        private void GetMinMaxXYZOfTriangle()
        {
            MinX = Math.Min(P1.X, Math.Min(P2.X, P3.X));
            MinY = Math.Min(P1.Y, Math.Min(P2.Y, P3.Y));
            MinZ = Math.Min(P1.Z, Math.Min(P2.Z, P3.Z));

            MaxX = Math.Max(P1.X, Math.Max(P2.X, P3.X));
            MaxY = Math.Max(P1.Y, Math.Max(P2.Y, P3.Y));
            MaxZ = Math.Max(P1.Z, Math.Max(P2.Z, P3.Z));
        }
    }
}