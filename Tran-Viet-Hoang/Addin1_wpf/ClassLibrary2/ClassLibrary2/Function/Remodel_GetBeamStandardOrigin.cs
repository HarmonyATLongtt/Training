using Autodesk.Revit.DB;
using System;

namespace ClassLibrary2.Function
{
    public class Remodel_GetBeamStandardOrigin
    {
        public XYZ BotBeamStandardOrigin(FamilyInstance elem, double cover, double stirrup)
        {
            BoundingBoxXYZ boundingbox = elem.get_BoundingBox(null);
            XYZ origin = XYZ.Zero;
            XYZ xVec = new Remodel_SetBeamStandard().xVecBeam(elem);
            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Max.Y - cover - stirrup, boundingbox.Min.Z + cover + stirrup);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Min.Y + cover + stirrup, boundingbox.Min.Z + cover + stirrup);
            }

            return origin;
        }

        public XYZ TopBeamStandardOrigin(FamilyInstance elem, double cover, double stirrup)
        {
            BoundingBoxXYZ boundingbox = elem.get_BoundingBox(null);
            XYZ origin = XYZ.Zero;
            // lấy về phương của thép dọc nằm trong elem
            XYZ xVec = new Remodel_SetBeamStandard().xVecBeam(elem);

            // điều kiện kiểm tra nếu thép đặt theo phương X
            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Max.Y - cover - stirrup, boundingbox.Max.Z  - cover - stirrup);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Min.Y + cover + stirrup, boundingbox.Max.Z - cover - stirrup);
            }

            return origin;
        }

    }
}