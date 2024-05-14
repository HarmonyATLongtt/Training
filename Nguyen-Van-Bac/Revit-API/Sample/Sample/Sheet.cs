using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Sample
{
    [Transaction(TransactionMode.Manual)]
    public class CreateSheet : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Bắt đầu một giao dịch
            using (Transaction t = new Transaction(doc, "Create Sheet"))
            {
                t.Start();

                FamilySymbol titleBlock = null;
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector.OfCategory(BuiltInCategory.OST_TitleBlocks).OfClass(typeof(FamilySymbol));

                foreach (FamilySymbol symbol in collector)
                {
                    titleBlock = symbol;
                    break;
                }

                if (titleBlock == null)
                {
                    message = "No title block found.";
                    return Result.Failed;
                }

                if (!titleBlock.IsActive)
                {
                    titleBlock.Activate();
                    doc.Regenerate();
                }

                ViewSheet newSheet = ViewSheet.Create(doc, titleBlock.Id);
                if (newSheet == null)
                {
                    message = "Failed to create new sheet.";
                    return Result.Failed;
                }

                newSheet.Name = " Sheet mới";
                newSheet.SheetNumber = "A1010";

                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}