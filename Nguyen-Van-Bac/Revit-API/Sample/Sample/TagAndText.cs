using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class CreateTag : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Create Tag");

                View view = doc.ActiveView;

                Reference pickedObj = uidoc.Selection.PickObject(ObjectType.Element, "Select an element to tag");
                if (pickedObj == null)
                {
                    message = "No element selected.";
                    return Result.Failed;
                }

                XYZ tagHeadPosition = new XYZ(0, 0, 0);

                IndependentTag tag = IndependentTag.Create(doc, view.Id, new Reference(doc.GetElement(pickedObj.ElementId)),
                    false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, tagHeadPosition);

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CreateTextNote : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Create Text Note");

                View view = doc.ActiveView;

                XYZ point = new XYZ(10, 10, 0);

                TextNoteType textNoteType = new FilteredElementCollector(doc)
                    .OfClass(typeof(TextNoteType))
                    .FirstElement() as TextNoteType;

                TextNote textNote = TextNote.Create(doc, view.Id, point, "Test", textNoteType.Id);

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}