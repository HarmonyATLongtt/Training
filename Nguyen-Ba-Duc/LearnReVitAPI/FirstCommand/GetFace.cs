using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class GetFace : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Lấy document đang mở
            UIDocument uIDocument = commandData.Application.ActiveUIDocument;
            Document doc = uIDocument.Document;

            try
            {
                // Chọn đối tượng tường
                Reference pickedRef = uIDocument.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Chọn một tường");
                if (pickedRef != null)
                {
                    // Lấy Element của đối tượng tường
                    Element element = doc.GetElement(pickedRef);
                    if (!(element is Wall wall))
                    {
                        TaskDialog.Show("Error", "Đối tượng được chọn không phải là tường.");
                        return Result.Failed;
                    }
                    // Lấy tọa độ đầu và cuối của tường
                    LocationCurve locationCurve = wall.Location as LocationCurve;
                    if (locationCurve == null)
                    {
                        TaskDialog.Show("Error", "Không thể lấy vị trí của tường.");
                        return Result.Failed;
                    }
                    XYZ startPoint = locationCurve.Curve.GetEndPoint(0);
                    XYZ endPoint = locationCurve.Curve.GetEndPoint(1);

                    // Hiển thị tọa độ đầu và cuối
                    TaskDialog.Show("Wall Info", $"Start Point: ({startPoint.X}, {startPoint.Y}, {startPoint.Z})\n" +
                                                 $"End Point: ({endPoint.X}, {endPoint.Y}, {endPoint.Z})");
                    // Lấy danh sách tất cả các mặt (faces) của tường
                    GeometryElement geometryElement = wall.get_Geometry(new Options());
                    List<Face> faces = new List<Face>();
                    foreach (GeometryObject obj in geometryElement)
                    {
                        if (obj is Solid solid)
                        {
                            foreach (Face face in solid.Faces)
                            {
                                faces.Add(face);
                            }
                        }
                    }

                    // Duyệt qua từng mặt và lấy thông tin các cạnh
                    foreach (Face face in faces)
                    {
                        // Lấy danh sách cạnh (edges) của mặt
                        EdgeArray edgeArray = face.EdgeLoops.get_Item(0);
                        foreach (Edge edge in edgeArray)
                        {
                            Curve edgeCurve = edge.AsCurve();

                            // Lấy tọa độ đầu và cuối của cạnh
                            XYZ edgeStart = edgeCurve.GetEndPoint(0);
                            XYZ edgeEnd = edgeCurve.GetEndPoint(1);

                            // Hiển thị thông tin cạnh
                            TaskDialog.Show("Edge Info", $"Edge Start: ({edgeStart.X}, {edgeStart.Y}, {edgeStart.Z})\n" +
                                                         $"Edge End: ({edgeEnd.X}, {edgeEnd.Y}, {edgeEnd.Z})");
                        }
                    }

                    return Result.Succeeded;
                }
                else
                {
                    return Result.Cancelled;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}