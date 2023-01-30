using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using ClassLibrary2.Data;
using System.Collections.Generic;
using System.Linq;

namespace ClassLibrary2.Function
{
    public class Remodel_MoveStirrup
    {
        //hàm di chuyển các stirrup vừa set lại giá trị, về đúng vị trí nằm trong cột đồng thời rải stirrup đi hết cấu kiện
        public void MoveStirrup(Document doc, List<ConcreteColumnData> columns, List<ConcreteBeamData> beams)
        {
            List<FamilyInstance> cols = new Remodel_GetFrame().EtabsColumns(doc, columns);
            List<FamilyInstance> framings = new Remodel_GetFrame().EtabsBeams(doc, beams);

            List<RebarSetData> colstirrups = new Remodel_GetStirrup().ColumnStirrup(doc, cols);
            List<RebarSetData> beamstirrups = new Remodel_GetStirrup().BeamStirrup(doc, framings);

            double cover = 50 / 304.8;
            using (Transaction trans = new Transaction(doc, "create stirrup"))
            {
                trans.Start();
                //cột
                foreach (var stirrup in colstirrups)
                {
                    BoundingBoxXYZ boundingbox = stirrup.Host_boundingbox_1;
                    XYZ min = boundingbox.Transform.OfPoint(boundingbox.Min);
                    XYZ origin = min + XYZ.BasisX * cover + XYZ.BasisY * cover + XYZ.BasisZ * cover;

                    BoundingBoxXYZ boundingboxnew = stirrup.ColumnStirrup.get_BoundingBox(null);
                    XYZ origin1 = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);
                    XYZ vect = origin - origin1;

                    ElementTransformUtils.MoveElement(doc, stirrup.ColumnStirrup.Id, vect);
                    stirrup.ColumnStirrup.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(250 / 304.8, stirrup.HostLength, true, true, false);
                }
                //dầm
                foreach (var stirrup in beamstirrups)
                {
                    XYZ origin = stirrup.BeamStirrupOrigin;

                    BoundingBoxXYZ boundingboxnew = stirrup.BeamStirrup.get_BoundingBox(null);
                    XYZ origin1 = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);
                    XYZ vect = origin - origin1;

                    ElementTransformUtils.MoveElement(doc, stirrup.BeamStirrup.Id, vect);
                    stirrup.BeamStirrup.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(250 / 304.8, stirrup.HostLength, true, true, false);
                }
                trans.Commit();
            }
        }
    }
}
