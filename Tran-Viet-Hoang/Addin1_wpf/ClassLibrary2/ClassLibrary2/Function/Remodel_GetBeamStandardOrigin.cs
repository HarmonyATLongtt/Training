using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using System;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_GetBeamStandardOrigin
    {
        public XYZ BotBeamStandardOrigin(FamilyInstance elem, double cover, double stirrup)
        {
            BoundingBoxXYZ boundingbox = elem.get_BoundingBox(null);
            XYZ origin = XYZ.Zero;
            XYZ xVec = xVecBeam(elem);
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
            XYZ xVec = xVecBeam(elem);
            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Max.Y - cover - stirrup, boundingbox.Max.Z - cover - stirrup);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                origin = new XYZ(boundingbox.Min.X + cover + stirrup, boundingbox.Min.Y + cover + stirrup, boundingbox.Max.Z - cover - stirrup);
            }

            return origin;
        }

        public XYZ xVecBeam(FamilyInstance elem)
        {
            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            Location loc = elem.Location;
            LocationCurve locCur = loc as LocationCurve;
            Curve curve = locCur.Curve;
            Line locline = curve as Line;

            //gán vector phương X trong definition của shape thép với vector vẽ cấu kiện
            XYZ xVec = locline.Direction;
            return xVec;
        }
    }
}
