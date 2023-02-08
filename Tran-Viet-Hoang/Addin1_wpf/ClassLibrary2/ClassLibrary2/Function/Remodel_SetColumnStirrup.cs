using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using ClassLibrary2.Factory.RebarSet;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_SetColumnStirrup
    {
        // hàm tạo 1 stirrup cho nhiều cột và set lại giá trị stirrup đó sao cho phù hợp với kích thước cột
        public void drawcolstirrup(Document doc, List<ConcreteColumnData> cols)
        {
            using (Transaction trans = new Transaction(doc, "create col stirrup"))
            {
                trans.Start();
                foreach (var col in cols)
                {
                    stirrupcolumnbefore(doc, col);
                }
                trans.Commit();
            }
        }

        //Hàm tạo 1 stirrup ban đầu cho 1 cột
        public Rebar stirrupcolumnbefore(Document doc, ConcreteColumnData coletabs)
        {
            RebarSetData stirrup = new RebarSet(doc).CalculateColumnRebar(coletabs).FirstOrDefault(x => x.Style == RebarStyle.StirrupTie);

            XYZ yVec = new XYZ(0, 1, 0);
            XYZ xVec = new XYZ(1, 0, 0);

            Rebar rebar = Rebar.CreateFromRebarShape(doc,
                stirrup.ShapeData.Shape,
                stirrup.Rebartype,
                coletabs.Host,
                stirrup.LocationData.RebarOrigin, xVec, yVec);
            foreach (var para in stirrup.ShapeData.Segments)
            {
                Parameter dim = rebar.LookupParameter(para.Key);
                dim.Set(para.Value);
            }
            return rebar;
        }
    }
}