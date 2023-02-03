using Autodesk.Revit.DB;
using ClassLibrary2.Data;
using ClassLibrary2.Data.FrameData;
using System.Collections.Generic;

namespace ClassLibrary2.Function
{
    public class Remodel_MoveStirrup
    {
        //hàm di chuyển các stirrup vừa set lại giá trị, về đúng vị trí nằm trong cột đồng thời rải stirrup đi hết cấu kiện
        public void MoveStirrup(Document doc, List<ConcreteColumnData> columns, List<ConcreteBeamData> beams)
        {
            //lấy về list các thép đai đã vẽ
            List<RebarSetData> colstirrups = new Remodel_GetStirrup().ColumnStirrup(doc, columns);
            List<RebarSetData> beamstirrups = new Remodel_GetStirrup().BeamStirrup(doc, beams);

            double cover = new ConcreteHostData().Covers.Side;
            using (Transaction trans = new Transaction(doc, "create stirrup"))
            {
                trans.Start();
                //cột

                foreach (var stirrup in colstirrups)
                {   //lấy vị trí điểm gốc cũ
                    BoundingBoxXYZ boundingbox = stirrup.LocationData.BoundingBox;
                    XYZ min = boundingbox.Transform.OfPoint(boundingbox.Min);
                    XYZ origin = min + XYZ.BasisX * cover + XYZ.BasisY * cover + XYZ.BasisZ * cover;

                    // lấy vị trí điểm gốc mới
                    BoundingBoxXYZ boundingboxnew = stirrup.ColumnStirrup.get_BoundingBox(null);
                    XYZ origin1 = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);

                    // tạo vect di chuyển từ vị trí mới về vị trí cũ
                    XYZ vect = origin - origin1;

                    // di chuyển thép đai theo vector vừa tạo
                    ElementTransformUtils.MoveElement(doc, stirrup.ColumnStirrup.Id, vect);

                    // set spacing cho thép đai sau khi thép đai trở về đúng vị trí
                    stirrup.ColumnStirrup.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(250 / 304.8, stirrup.HostData.HostLength, true, true, false);
                }
                //dầm
                foreach (var stirrup in beamstirrups)
                {
                    XYZ origin1st = stirrup.LocationData.RebarOrigin;

                    BoundingBoxXYZ boundingboxnew = stirrup.BeamStirrup.get_BoundingBox(null);
                    XYZ origin2nd = boundingboxnew.Transform.OfPoint(boundingboxnew.Min);
                    XYZ vect = origin1st - origin2nd;

                    ElementTransformUtils.MoveElement(doc, stirrup.BeamStirrup.Id, vect);
                    stirrup.BeamStirrup.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(250 / 304.8, stirrup.HostData.HostLength, true, true, false);
                }
                trans.Commit();
            }
        }
    }
}