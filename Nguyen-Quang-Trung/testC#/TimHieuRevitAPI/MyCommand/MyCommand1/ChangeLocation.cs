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
    internal class ChangeLocation : IExternalCommand
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

                // get element
                ElementId elementId = refer.ElementId;
                Element element = doc.GetElement(elementId);

                if (refer != null)
                {
                    using (Transaction trans = new Transaction(doc, "change location"))
                    {
                        trans.Start();

                        LocationPoint loc = element.Location as LocationPoint;
                        XYZ curPoint = loc.Point;
                        XYZ newPoint = new XYZ(curPoint.X + 5, curPoint.Y, curPoint.Z);
                        loc.Point = newPoint;

                        trans.Commit();
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