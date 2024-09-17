using Autodesk.Revit.Attributes;
using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace BaiTap_Revit.P5
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class Text : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Pick a point for the text
                XYZ point = uidoc.Selection.PickPoint("Select the location for the text");

                using (Transaction tx = new Transaction(doc, "Create Text"))
                {
                    tx.Start();

                    // Get a valid text type from the document
                    TextNoteType textType = new FilteredElementCollector(doc)
                        .OfClass(typeof(TextNoteType))
                        .FirstOrDefault() as TextNoteType;

                    if (textType == null)
                    {
                        message = "No valid TextNoteType found in the document.";
                        return Result.Failed;
                    }

                    // Define the text options with the valid text type
                    TextNoteOptions textOptions = new TextNoteOptions(textType.Id)
                    {
                        HorizontalAlignment = HorizontalTextAlignment.Left,
                        VerticalAlignment = VerticalTextAlignment.Middle
                    };

                    // Create the text note
                    TextNote textNote = TextNote.Create(doc, doc.ActiveView.Id, point, "Sample Text", textOptions);

                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }

            return Result.Succeeded;
        }
    }
}