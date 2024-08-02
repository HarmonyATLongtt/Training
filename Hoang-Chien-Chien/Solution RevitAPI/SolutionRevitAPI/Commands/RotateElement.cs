using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class RotateElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
                Element element = doc.GetElement(reference);

                if (reference != null)
                {
                    using (Transaction trans = new Transaction(doc, "Transform"))
                    {
                        trans.Start();

                        //Rotate Element
                        XYZ point1 = new XYZ();
                        XYZ point2 = new XYZ();
                        if (element.Location is LocationCurve)
                        {
                            LocationCurve line = element.Location as LocationCurve;
                            Line curve = line.Curve as Line;
                            double centerX = (curve.GetEndPoint(0).X + curve.GetEndPoint(1).X) / 2;
                            double centerY = (curve.GetEndPoint(0).Y + curve.GetEndPoint(1).Y) / 2;
                            double centerZ = (curve.GetEndPoint(0).X + curve.GetEndPoint(1).Z) / 2;
                            point1 = new XYZ(centerX, centerY, centerZ);
                            point2 = new XYZ(centerX, centerY, centerZ + 1);
                        }
                        else
                        {
                            LocationPoint loc = element.Location as LocationPoint;
                            XYZ point = loc.Point as XYZ;
                            point1 = new XYZ(point.X, point.Y, point.Z);
                            point2 = new XYZ(point.X, point.Y, point.Z + 1);
                        }
                        Line axis = Line.CreateBound(point1, point2);
                        double angle = 45 * Math.PI / 180;
                        ElementTransformUtils.RotateElement(doc, element.Id, axis, angle);
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}