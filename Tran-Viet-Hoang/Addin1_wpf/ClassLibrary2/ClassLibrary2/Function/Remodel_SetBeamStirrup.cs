using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using ClassLibrary2.Data.FrameData;
using System;
using System.Collections.Generic;

namespace ClassLibrary2.Function
{
    public class Remodel_SetBeamStirrup
    {
        // hàm tạo 1 stirrup cho nhiều dầm và set lại giá trị stirrup đó sao cho phù hợp với kích thước dầm
        public void drawbeamstirrup(Document doc, List<ConcreteBeamData> beams)
        {
            double cover = new ConcreteHostData().Covers.Side;
          

            using (Transaction trans = new Transaction(doc, "create beam stirrup"))
            {
                trans.Start();
                foreach (var beametabs in beams)
                {
                    Rebar barnew = stirrupbeambefore(beametabs, doc);
                    Parameter tie_B = barnew.LookupParameter("B");
                    Parameter tie_C = barnew.LookupParameter("C");
                    Parameter tie_D = barnew.LookupParameter("D");
                    Parameter tie_E = barnew.LookupParameter("E");

                    double B_D = beametabs.Dimensions.b - 2 * cover;
                    tie_B.Set(B_D);
                    tie_D.Set(B_D);

                    double C_E = beametabs.Dimensions.h - 2 * cover;
                    tie_C.Set(C_E);
                    tie_E.Set(C_E);
                }
                trans.Commit();
            }
        }

        //Hàm tạo 1 stirrup ban đầu cho 1 dầm
        public Rebar stirrupbeambefore(ConcreteBeamData beametabs, Document doc )
        {

            string rebartype = "8M";
            string rebarshape = "M_T1";

            RebarShape shape = new Remodel_GetElem().GetRebarShape(doc, rebarshape);
            RebarBarType type = new Remodel_GetElem().GetRebarBarType(doc, rebartype);

            //Lấy hướng vẽ của cấu kiện để biết là sẽ vẽ thép cho cấu kiện theo phương X hay pương Y
            XYZ xVec = beametabs.drawdirection; // để lấy được chiều vẽ của dầm
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
            XYZ origin = new Remodel_GetStirrup().FrameStirrupOrigin(beametabs);
            Rebar rebar = Rebar.CreateFromRebarShape(doc, 
                beametabs.Stirrup_Tie.ShapeData.Shape,
                beametabs.Stirrup_Tie.DiameterData.Rebartype, 
                beametabs.Host,
                origin, xVec, yVec);
            return rebar;
        }
    }
}