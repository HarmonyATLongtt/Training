using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

using FirstCommand.Support.Constants;
using FirstCommand.Support.PointHandle;

namespace FirstCommand.Support.PlaneHandle
{
    public static class PlaneUtility
    {
        /// <summary>
        /// Hàm tạo plane song song với trục Z từ 1 line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Plane CreatePlaneParallelToZFromLine(Line line)
        {
            XYZ dir = line.Direction.Normalize();
            double dot = dir.DotProduct(XYZ.BasisZ);
            if (Math.Abs(Math.Abs(dot) - 1) > CommonConstants.TOLERANCE)
            {
                XYZ p1 = line.GetEndPoint(0);
                XYZ p2 = line.GetEndPoint(1);
                XYZ p3 = PointUtility.SetPointWithNewZValue(p1, 0);
                Plane plane = Plane.CreateByThreePoints(p1, p2, p3);
                return plane;
            }
            return null;
        }

        /// <summary>
        /// Hàm này dùng để tìm ra hình chiếu của 1 điểm lên trên 1 plane
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static XYZ GetProjectedPoint(Plane plane, XYZ point)
        {
            XYZ planeOrigin = plane.Origin;
            XYZ planeNormal = plane.Normal.Normalize();
            XYZ pointToOrigin = point - planeOrigin;

            // Tính khoảng cách từ điểm đến mặt phẳng (dọc theo pháp tuyến)
            double distance = pointToOrigin.DotProduct(planeNormal);

            return point - distance * planeNormal;
        }

        /// <summary>
        /// Hàm này kiểm tra xem 1 điểm có nằm trên 1 mặt phẳng không, với sai số cho trước
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool IsPointOnPlane(Plane plane, XYZ point)
        {
            double distance = plane.Normal.DotProduct(point - plane.Origin);
            return Math.Abs(distance) < 0.1;
            //return Math.Abs(distance) < defaultRadius;
        }
    }
}