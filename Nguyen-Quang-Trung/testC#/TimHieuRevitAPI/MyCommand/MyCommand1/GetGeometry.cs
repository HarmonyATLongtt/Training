using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace MyCommand1
{
    [TransactionAttribute(TransactionMode.Manual)] // giúp revit hiểu cách đọc lệnh này như thế nào
    internal class GetGeometry : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // get uidocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // get document
            Document doc = uidoc.Document;
            try
            {
                // get reference element
                Reference refer = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (refer != null)
                {
                    // get element
                    ElementId elementId = refer.ElementId;
                    Element element = doc.GetElement(elementId);

                    //get geometry
                    Options opt = new Options();
                    opt.DetailLevel = ViewDetailLevel.Coarse;
                    GeometryElement geoElement = element.get_Geometry(opt);

                    foreach (GeometryObject obj in geoElement)
                    {
                        Solid solid = obj as Solid;
                        int faces = 0;
                        double areas = 0.0;
                        foreach (Face f in solid.Faces)
                        {
                            areas += f.Area;
                            faces++;
                        }
                        TaskDialog.Show("Geometry", string.Format("Tuong da chon co so mat la {0}, tong dien tich cac mat la {1}", faces, UnitUtils.ConvertFromInternalUnits(areas, DisplayUnitType.DUT_SQUARE_METERS)));
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}