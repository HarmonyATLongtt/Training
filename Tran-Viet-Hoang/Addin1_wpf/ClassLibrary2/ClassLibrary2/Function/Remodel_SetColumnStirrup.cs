using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_SetColumnStirrup
    {
        // hàm tạo 1 stirrup cho nhiều cột và set lại giá trị stirrup đó sao cho phù hợp với kích thước cột
        public void drawcolstirrup(Document doc, List<ConcreteColumnData> cols)
        {
            List<FamilyInstance> coletabselems = new Remodel_GetFrame().EtabsColumns(doc, cols);
            double cover = 50 / 304.8;
            RebarShape shape = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarShape))
                .Cast<RebarShape>()
                .First(x => x.Name == "M_T1");

            RebarBarType type = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .First(x => x.Name == "8M");

            using (Transaction trans = new Transaction(doc, "create col stirrup"))
            {
                trans.Start();
                foreach (var coletabs in coletabselems)
                {
                    Rebar barnew = stirrupcolumnbefore(coletabs, doc, shape, type);
                    Parameter tie_B = barnew.LookupParameter("B");
                    Parameter tie_C = barnew.LookupParameter("C");
                    Parameter tie_D = barnew.LookupParameter("D");
                    Parameter tie_E = barnew.LookupParameter("E");

                    double B_D = coletabs.Symbol.LookupParameter("b").AsDouble() - 2 * cover;
                    tie_B.Set(B_D);
                    tie_D.Set(B_D);

                    double C_E = coletabs.Symbol.LookupParameter("h").AsDouble() - 2 * cover;
                    tie_C.Set(C_E);
                    tie_E.Set(C_E);
                }
                trans.Commit();
            }
        }

        //Hàm tạo 1 stirrup ban đầu cho 1 cột
        public Rebar stirrupcolumnbefore(FamilyInstance coletabs, Document doc, RebarShape shape, RebarBarType type)
        {
            double cover = 50 / 304.8;

            BoundingBoxXYZ boundingbox = coletabs.get_BoundingBox(null);

            XYZ origin = new XYZ(boundingbox.Min.X + cover, boundingbox.Min.Y + cover, boundingbox.Min.Z + cover);

            XYZ yVec = new XYZ(0, 1, 0);
            XYZ xVec = new XYZ(1, 0, 0);

            Rebar rebar = Rebar.CreateFromRebarShape(doc, shape, type, coletabs, origin, xVec, yVec);
            return rebar;
        }
    }
}
