using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace Bai2
{
    [Transaction(TransactionMode.ReadOnly)]
    internal class Geometry_Solid : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (r != null)
                {
                    ElementId elementid = r.ElementId;
                    Element element = doc.GetElement(elementid);
                    Options opt = new Options();
                    opt.DetailLevel = ViewDetailLevel.Fine;
                    GeometryElement geometryElement = element.get_Geometry(opt);
                    foreach (GeometryObject obj in geometryElement)
                    {
                        Solid solid = obj as Solid;
                        int face = 0, edges = 0;
                        double areas = 0.0;

                        foreach (Edge e in solid.Edges)
                        {
                            edges++;
                        }
                        foreach (Face f in solid.Faces)
                        {
                            face++;
                            areas += f.Area;
                        }
                        if (face > 0)
                        TaskDialog.Show("Geometry","So canh: " + edges + "\n" +
                                        "So mat: " + face + "\n" +
                                        "Tong dien tich: " + UnitUtils.ConvertFromInternalUnits(areas, DisplayUnitType.DUT_SQUARE_METERS) + "m²");
                    }
                }
                return Result.Succeeded;
            }
            catch(Exception ex)
            {   
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
