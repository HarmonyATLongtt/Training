using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

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
                var sheetNumbers = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSheet))
                    .Cast<ViewSheet>()
                    .Select(sheet => sheet.SheetNumber)
                    .Where(sn => int.TryParse(sn, out _))
                    .Select(sn => int.Parse(sn))
                    .ToList();

                // Tìm mã số lớn nhất hiện tại
                int maxSheetNumber = sheetNumbers.Any() ? sheetNumbers.Max() : 0;
                // Tạo mã số mới tăng dần
                int newSheetNumber = maxSheetNumber++;
                string newSheetNumberStr = newSheetNumber.ToString();
                // Tạo mã số mới
                newSheet.Name = " Sheet mới";
                newSheet.SheetNumber = newSheetNumber.ToString();

                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}