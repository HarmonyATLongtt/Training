using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateTag : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Reference reference = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Pick an object to cretae new tag for it...");
            var element = doc.GetElement(reference);

            TagMode tagMode = TagMode.TM_ADDBY_CATEGORY;
            TagOrientation tagOrientation = TagOrientation.Horizontal;

            try
            {
                using (var trans = new Transaction(doc, "Create new tag"))
                {
                    trans.Start();

                    if (element is Room)
                    {
                        var point = (element.Location as LocationPoint).Point;
                        doc.Create.NewRoomTag(new LinkElementId(new ElementId(element.Id.IntegerValue)), new UV(point.X, point.Y), doc.ActiveView.Id);
                    }
                    else
                    {
                        var position = new XYZ();
                        if (element.Location is LocationPoint point)
                        {
                            position = point.Point;
                        }
                        else if (element.Location is LocationCurve curve)
                        {
                            position = (curve.Curve as Line).Origin;
                        }

                        IndependentTag tag = IndependentTag.Create(doc, doc.ActiveView.Id, reference, true, tagMode, tagOrientation, position);
                    }

                    trans.Commit();
                }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}