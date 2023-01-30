using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_SetBeamStirrup
    {
        // hàm tạo 1 stirrup cho nhiều cột và set lại giá trị stirrup đó sao cho phù hợp với kích thước dầm
        public void drawbeamstirrup(Document doc, List<ConcreteBeamData> beams)
        {
            List<FamilyInstance> beametabselems = new Remodel_GetFrame().EtabsBeams(doc, beams);
            double cover = 50 / 304.8;
            RebarShape shape = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_T1");

            RebarBarType type = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == "8M");

            using (Transaction trans = new Transaction(doc, "create beam stirrup"))
            {
                trans.Start();
                foreach (var beametabs in beametabselems)
                {
                    Rebar barnew = stirrupbeambefore(beametabs, doc, shape, type);
                    Parameter tie_B = barnew.LookupParameter("B");
                    Parameter tie_C = barnew.LookupParameter("C");
                    Parameter tie_D = barnew.LookupParameter("D");
                    Parameter tie_E = barnew.LookupParameter("E");

                    double B_D = beametabs.Symbol.LookupParameter("b").AsDouble() - 2 * cover;
                    tie_B.Set(B_D);
                    tie_D.Set(B_D);

                    double C_E = beametabs.Symbol.LookupParameter("h").AsDouble() - 2 * cover;
                    tie_C.Set(C_E);
                    tie_E.Set(C_E);
                }
                trans.Commit();
            }
        }



        //Hàm tạo 1 stirrup ban đầu cho 1 dầm
        public Rebar stirrupbeambefore(FamilyInstance beametabs, Document doc, RebarShape shape, RebarBarType type)
        {
            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            Location loc = beametabs.Location;
            LocationCurve locCur = loc as LocationCurve;
            Curve curve = locCur.Curve;
            Line locline = curve as Line;
            XYZ xVec = locline.Direction; // để lấy được chiều vẽ của dầm
            //khai báo giá trị other cover, để xác định chính xác length của thép
            double cover = 50 / 304.8;

            XYZ yVec = new XYZ(0, 0, 1);
            if (Math.Abs(xVec.X) > Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương X, thì phương X của family thép đai sẽ map vào phương Y
                xVec = new XYZ(0, 1, 0);
            }
            else if (Math.Abs(xVec.X) < Math.Abs(xVec.Y))
            {
                //Nếu dầm được vẽ theo phương Y, thì phương X của family thép đai sẽ map vào phương X
                xVec = new XYZ(1, 0, 0);
            }
            XYZ origin = new Remodel_GetStirrup().FrameStirrupOrigin(beametabs, cover);
            Rebar rebar = Rebar.CreateFromRebarShape(doc, shape, type, beametabs, origin, xVec, yVec);
            return rebar;
        }

    }
}
