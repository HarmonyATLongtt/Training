using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreateText : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Reference r = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

            try
            {
                if (r != null)
                {
                    using (Transaction trans = new Transaction(doc, "Create Text & Tag"))
                    {
                        trans.Start();
                        ElementId elementId = r.ElementId;
                        Element element = doc.GetElement(elementId);
                        XYZ textPosition = element.Location is LocationPoint locPoint ? locPoint.Point : new XYZ(0, 0, 0);

                        TextNoteType textNoteType = new FilteredElementCollector(doc)
                            .OfClass(typeof(TextNoteType))
                            .Cast<TextNoteType>()
                            .FirstOrDefault();

                        TextNote.Create(doc, doc.ActiveView.Id, textPosition, element.Name, textNoteType.Id);
                        IndependentTag.Create(doc, doc.ActiveView.Id, new Reference(element), false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, textPosition);

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