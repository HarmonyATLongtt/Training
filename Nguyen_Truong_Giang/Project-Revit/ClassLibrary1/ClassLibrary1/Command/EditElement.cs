using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace ClassLibrary1
{
    [TransactionAttribute(TransactionMode.Manual)]

    class EditElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get Document
            Document doc = uidoc.Document;

            try
            {
                //get Reference Element
                Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                //get Element
                ElementId elementId = r.ElementId;
                Element element = doc.GetElement(elementId);

                if (r != null)
                {
                    using (Transaction trans = new Transaction(doc, "Set Para"))
                    {
                        trans.Start();

                        //move Element
                        XYZ vectorMove = new XYZ(4, 4,0);
                        ElementTransformUtils.MoveElement(doc, elementId, vectorMove);

                        //rotate Element
                        LocationPoint loc = element.Location as LocationPoint;
                        XYZ p1 = loc.Point;
                        XYZ p2 = new XYZ(p1.X,p1.Y,p1.Z + 3);
                        Line axis = Line.CreateBound(p1, p2);
                        double angle = 45 * Math.PI / 180;
                        ElementTransformUtils.RotateElement(doc, elementId, axis, angle);

                        trans.Commit();
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
