using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace ClassLibrary1
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class ChangeLocation : IExternalCommand
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

                        LocationPoint loc = element.Location as LocationPoint;

                        XYZ curPoint = loc.Point;

                        XYZ newPoint = new XYZ(curPoint.X + 4, curPoint.Y, curPoint.Z);

                        loc.Point = newPoint;

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