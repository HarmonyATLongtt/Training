using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                var r1 = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Face);
                var r2 = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Face);

                var e1 = doc.GetElement(r1);
                var e2 = doc.GetElement(r2);

                var g1 = e1.GetGeometryObjectFromReference(r1) as PlanarFace;
                var g2 = e2.GetGeometryObjectFromReference(r2) as PlanarFace;
                _ = new UV();
                var point1 = g1.Origin;
                Plane plane = Plane.CreateByNormalAndOrigin(g2.FaceNormal, g2.Origin);
                double distance;
                plane.Project(point1, out _, out distance);

                double angle = plane.Normal.AngleTo(point1 - g2.Origin);
                XYZ point2;
                if (angle > 0)
                    point2 = point1 - plane.Normal * distance;
                else
                    point2 = -point1 + plane.Normal * distance;

                ReferenceArray references = new ReferenceArray();
                references.Append(r1);
                references.Append(r2);

                try
                {
                    using (var trans = new Transaction(doc, "Create new dimension"))
                    {
                        trans.Start();

                        doc.Create.NewDimension(doc.ActiveView, Line.CreateBound(point1, point2), references);

                        trans.Commit();
                    }
                }
                catch { }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}