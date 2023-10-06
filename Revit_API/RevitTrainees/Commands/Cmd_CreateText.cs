using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateText : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            XYZ position = uiDoc.Selection.PickPoint("Pick a point to place text...");
            ElementId defaultTextTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
            TextNoteOptions options = new TextNoteOptions(defaultTextTypeId);
            options.Rotation = 3.141592654f / 4;
            options.HorizontalAlignment = HorizontalTextAlignment.Center;
            options.VerticalAlignment = VerticalTextAlignment.Middle;

            string text = "Text note just create in here...";

            try
            {
                using (var trans = new Transaction(doc, "Create new text..."))
                {
                    trans.Start();

                    TextNote.Create(doc, doc.ActiveView.Id, position, text, options);

                    trans.Commit();
                }
            }
            catch { }

            return Result.Succeeded;
        }
    }
}