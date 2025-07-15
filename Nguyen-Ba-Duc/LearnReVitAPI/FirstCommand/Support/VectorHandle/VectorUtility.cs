using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using FirstCommand.Support.Constants;
using Microsoft.SqlServer.Server;

namespace FirstCommand.Support.VectorHandle
{
    public static class VectorUtility
    {
        /// <summary>
        /// Trả về vector đơn vị cùng hướng, hoặc null nếu độ dài gần bằng 0.
        /// </summary>
        /// <param name="v">Vector cần chuẩn hóa</param>
        /// <param name="tolerance">Ngưỡng độ dài nhỏ nhất chấp nhận</param>
        public static XYZ NormalizeSafe(XYZ v, double tolerance = 1e-10)
        {
            if (v == null) return null;

            double lengthSq = v.DotProduct(v);
            if (lengthSq < tolerance)
                return null;

            double length = Math.Sqrt(lengthSq);
            return v.Divide(length);
        }

        /// <summary>
        /// Hàm kiểm tra 2 vector có vuông góc không
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool ArePerpendicular(XYZ v1, XYZ v2, double tolerance = CommonConstants.TOLERANCE)
        {
            if (v1 == null || v2 == null)
                throw new ArgumentNullException();

            double dot = v1.DotProduct(v2);

            // Nếu dot gần 0 mà không phải do vector zero thì coi là vuông góc
            if (Math.Abs(dot) < tolerance)
            {
                double len1 = v1.GetLength();
                double len2 = v2.GetLength();

                if (len1 < 1e-12 || len2 < 1e-12)
                    return false;

                return true;
            }

            return false;
            //double dot = v1.DotProduct(v2);
            //if (Math.Abs(dot) < 1e-12) // shortcut cho vector zero
            //    return true;

            //double len1 = v1.GetLength();
            //double len2 = v2.GetLength();

            //if (len1 < 1e-12 || len2 < 1e-12)
            //    return false;  // vector gần như zero

            //return Math.Abs(dot) < tolerance;

            //if (v1.IsZeroLength() || v2.IsZeroLength())
            //    return false;  // Vector rỗng không có hướng xác định

            //double dot = v1.Normalize().DotProduct(v2.Normalize());

            //return Math.Abs(dot) < tolerance;
        }

        /// <summary>
        /// Hàm kiểm tra xem 2 vector có song song với nhau hay không
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool AreParallel(XYZ v1, XYZ v2, double tolerance = CommonConstants.TOLERANCE)
        {
            var cross = v1.CrossProduct(v2);

            return cross.GetLength() < tolerance;
        }

        /// <summary>
        /// Hàm dùng để kiểm tra xem 1 vector có song song với bất kỳ trục nào không, nếu có thì gán vector đó bằng trục đó
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="tolerance"></param>
        public static void IsParallelToAnyAxis(ref XYZ vector, double tolerance = CommonConstants.TOLERANCE)
        {
            List<XYZ> axises = new List<XYZ> { XYZ.BasisX, XYZ.BasisY, XYZ.BasisZ };
            foreach (var axis in axises)
            {
                var cross = vector.CrossProduct(axis);
                if (cross.GetLength() < tolerance)
                {
                    vector = axis;
                }
            }
        }
    }
}