using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_GetStirrup
    {
        // hàm tạo list trả về các giá trị kiểu Rebar, giá trị length, b, h và boundingbox của host column
        public List<RebarSetData> ColumnStirrup(Document doc, List<FamilyInstance> cols)
        {
            List<RebarSetData> rebars = new List<RebarSetData>();
            string style = "Stirrup / Tie";
            foreach (var col in cols)
            {
                var stirrup = new FilteredElementCollector(doc)
                  .WhereElementIsNotElementType()
                  .OfCategory(BuiltInCategory.OST_Rebar)
                  .Cast<Rebar>()
                  .First(x => x.LookupParameter("Style").AsValueString() == style && x.GetHostId() == col.Id);
                if (stirrup != null)
                {
                    RebarSetData rebar = new RebarSetData();
                    rebar.ColumnStirrup = stirrup;
                    rebar.HostLength = col.LookupParameter("Length").AsDouble();
                    rebar.Host_h = col.Symbol.LookupParameter("h").AsDouble();
                    rebar.Host_b = col.Symbol.LookupParameter("b").AsDouble();
                    rebar.Host_boundingbox_1 = col.get_BoundingBox(null);
                    rebars.Add(rebar);
                }
            }
            return rebars;
        }

        // hàm tạo list trả về các giá trị kiểu Rebar, giá trị length, b, h và boundingbox của host beam
        public List<RebarSetData> BeamStirrup(Document doc, List<FamilyInstance> beams)
        {
            List<RebarSetData> rebars = new List<RebarSetData>();
            string style = "Stirrup / Tie";
            foreach (var beam in beams)
            {
                var stirrup = new FilteredElementCollector(doc)
                  .WhereElementIsNotElementType()
                  .OfCategory(BuiltInCategory.OST_Rebar)
                  .Cast<Rebar>()
                  .First(x => x.LookupParameter("Style").AsValueString() == style && x.GetHostId() == beam.Id);
                if (stirrup != null)
                {
                    RebarSetData rebar = new RebarSetData();
                    rebar.BeamStirrup = stirrup;
                    rebar.HostLength = beam.LookupParameter("Length").AsDouble();
                    rebar.Host_h = beam.Symbol.LookupParameter("h").AsDouble();
                    rebar.Host_b = beam.Symbol.LookupParameter("b").AsDouble();
                    rebar.BeamStirrupOrigin = FrameStirrupOrigin(beam, 50 / 304.8);
                    rebars.Add(rebar);
                }
            }
            return rebars;
        }

        // Hàm lấy origin ban đầu về cho stirrup của dầm
        public XYZ FrameStirrupOrigin(FamilyInstance beam, double cover)
        {
            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            Location loc = beam.Location;
            LocationCurve locCur = loc as LocationCurve;
            Curve curve = locCur.Curve;
            Line locline = curve as Line;
            XYZ xVec = locline.Direction; // để lấy được chiều vẽ của dầm

            BoundingBoxXYZ boundingbox = beam.get_BoundingBox(null);

            XYZ origin = XYZ.Zero;

            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương X, thì phương X của family thép đai sẽ map vào phương Y
                origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Min.Z + cover);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương Y, thì phương X của family thép đai sẽ map vào phương X
                origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Max.Y - cover, boundingbox.Min.Z + cover);
            }
            return origin;
        }
    }
}
