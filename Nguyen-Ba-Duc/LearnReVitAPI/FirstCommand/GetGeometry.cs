using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.ReadOnly)]
    public class GetGeoMetry : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                // Get Referenct Element
                Reference r = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (r != null)
                {
                    // Get Element
                    ElementId elementId = r.ElementId;
                    Element element = doc.GetElement(elementId);

                    //Get geometry
                    Options options = new Options();
                    options.DetailLevel = ViewDetailLevel.Fine;
                    GeometryElement geometryElement = element.get_Geometry(options);

                    foreach (GeometryObject obj in geometryElement)
                    {
                        Solid solid = obj as Solid;
                        int faces = 0;
                        double areas = 0.0;

                        if (solid != null)
                        {
                            foreach (Face f in solid.Faces)
                            {
                                areas += f.Area;
                                faces++;
                            }
                            TaskDialog.Show("Geometry", string.Format("Wall da chon co so mat la {0}, tong dien tich cac mat la {1}"
                                , faces, areas));
                        }
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}