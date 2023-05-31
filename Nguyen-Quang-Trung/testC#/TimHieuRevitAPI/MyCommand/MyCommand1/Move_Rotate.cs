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
    internal class Move_Rotate : IExternalCommand
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

                        // move element
                        XYZ vectorMove = new XYZ(5, 5, 0);
                        ElementTransformUtils.MoveElement(doc, elementId, vectorMove);

                        // rotate element
                        LocationPoint loc = element.Location as LocationPoint;
                        XYZ p1 = loc.Point;
                        XYZ p2 = new XYZ(p1.X, p1.Y, p1.Z + 4);
                        Line axis = Line.CreateBound(p1, p2);
                        double angle = 45 * Math.PI / 180;
                        ElementTransformUtils.RotateElement(doc, elementId, axis, angle);

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