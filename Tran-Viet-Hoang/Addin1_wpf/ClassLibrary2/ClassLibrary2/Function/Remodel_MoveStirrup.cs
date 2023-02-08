using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using ClassLibrary2.Data.FrameData;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_MoveStirrup
    {
        //hàm di chuyển các stirrup vừa set lại giá trị, về đúng vị trí nằm trong cột đồng thời rải stirrup đi hết cấu kiện
        public void MoveStirrup(Document doc, List<ConcreteColumnData> columns, List<ConcreteBeamData> beams)
        {
            double cover = new ConcreteHostData().Covers.Side;
            using (Transaction trans = new Transaction(doc, "create stirrup"))
            {
                trans.Start();
                //cột
                foreach (var col in columns)
                {
                    var Ins = new Remodel_GetElem().GetStirrupTie(doc, col.Host as FamilyInstance);
                    RebarSetData stirrup = col.Rebars.FirstOrDefault(x => x.Style == RebarStyle.StirrupTie);
                    XYZ origin1st = stirrup.LocationData.RebarOrigin;

                    BoundingBoxXYZ boundingboxnew = Ins.get_BoundingBox(null);
                    XYZ origin2nd = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);
                    XYZ vect = origin1st - origin2nd;

                    ElementTransformUtils.MoveElement(doc, Ins.Id, vect);
                    Ins.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(stirrup.LayoutData.Spacing, col.Length, true, true, false);
                }
                //dầm
                foreach (var beam in beams)
                {
                    var Ins = new Remodel_GetElem().GetStirrupTie(doc, beam.Host as FamilyInstance);
                    RebarSetData stirrup = beam.Rebars.FirstOrDefault(x => x.Style == RebarStyle.StirrupTie);
                    XYZ origin1st = stirrup.LocationData.RebarOrigin;

                    BoundingBoxXYZ boundingboxnew = Ins.get_BoundingBox(null);
                    XYZ origin2nd = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);
                    XYZ vect = origin1st - origin2nd;

                    ElementTransformUtils.MoveElement(doc, Ins.Id, vect);
                    Ins.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(stirrup.LayoutData.Spacing, beam.Length, true, true, false);
                }
                trans.Commit();
            }
        }
    }
}