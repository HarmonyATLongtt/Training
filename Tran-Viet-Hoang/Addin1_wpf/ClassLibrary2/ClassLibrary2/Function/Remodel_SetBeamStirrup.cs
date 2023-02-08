using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using ClassLibrary2.Factory.RebarSet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_SetBeamStirrup
    {
        // hàm tạo 1 stirrup cho nhiều dầm và set lại giá trị stirrup đó sao cho phù hợp với kích thước dầm
        public void drawbeamstirrup(Document doc, List<ConcreteBeamData> beams)
        {
            using (Transaction trans = new Transaction(doc, "create beam stirrup"))
            {
                trans.Start();
                foreach (var beametabs in beams)
                {
                    stirrupbeambefore(beametabs, doc);
                }
                trans.Commit();
            }
        }

        //Hàm tạo 1 stirrup ban đầu cho 1 dầm
        public Rebar stirrupbeambefore(ConcreteBeamData beametabs, Document doc)
        {
            RebarSetData stirrup = new RebarSet(doc).CalculateBeamRebar(beametabs).FirstOrDefault(x => x.Style == RebarStyle.StirrupTie);

            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            XYZ xVec = beametabs.drawdirection;
            //khai báo giá trị other cover, để xác định chính xác length của thép

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

            Rebar rebar = Rebar.CreateFromRebarShape(doc,
                stirrup.ShapeData.Shape,
                stirrup.Rebartype,
                beametabs.Host,
                stirrup.LocationData.RebarOrigin, xVec, yVec);
            stirrup.Rebars = rebar;

            foreach (var para in stirrup.ShapeData.Segments)
            {
                Parameter dim = rebar.LookupParameter(para.Key);
                dim.Set(para.Value);
            }

            return rebar;
        }
    }
}