using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using ClassLibrary2.Data.FrameData;
using ClassLibrary2.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Factory.ReinforcingColumnSet
{
    public class CreatingColumnDesignRebar
    {
        // hàm tạo 1 stirrup cho nhiều cột và set lại giá trị stirrup đó sao cho phù hợp với kích thước cột
        public void CreateColumnRebar(Document doc, List<ConcreteColumnData> cols)
        {
            var rebarFactory = new RebarSet.RebarSet(doc);
            foreach (var col in cols)
                col.Rebars = rebarFactory.CalculateColumnRebar(col);

            using (Transaction trans = new Transaction(doc, "create col stirrup"))
            {
                trans.Start();
                foreach (var col in cols)
                {
                    Stirrupcolumnbefore(doc, col);
                }
                MoveStirrup(doc, cols);
                trans.Commit();
            }
        }

        //Hàm tạo 1 stirrup ban đầu cho 1 cột
        public Rebar Stirrupcolumnbefore(Document doc, ConcreteColumnData coletabs)
        {
            RebarSetData stirrup = new RebarSet.RebarSet(doc).CalculateColumnRebar(coletabs).FirstOrDefault(x => x.Style == RebarStyle.StirrupTie);

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

        public void MoveStirrup(Document doc, List<ConcreteColumnData> columns)
        {
            double cover = new ConcreteHostData().Covers.Side;

            //cột
            foreach (var col in columns)
            {
                var Ins = Common.GetStirrupTie(doc, col.Host as FamilyInstance);
                RebarSetData stirrup = col.Rebars.FirstOrDefault(x => x.Style == RebarStyle.StirrupTie);
                XYZ origin1st = stirrup.LocationData.RebarOrigin;

                BoundingBoxXYZ boundingboxnew = Ins.get_BoundingBox(null);
                XYZ origin2nd = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);
                XYZ vect = origin1st - origin2nd;

                ElementTransformUtils.MoveElement(doc, Ins.Id, vect);
                Ins.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(stirrup.LayoutData.Spacing, col.Length, true, true, false);
            }
        }
    }
}
