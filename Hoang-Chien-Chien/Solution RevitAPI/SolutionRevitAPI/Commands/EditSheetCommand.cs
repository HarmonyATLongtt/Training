using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SolutionRevitAPI.Commands.ExternalEvents;
using SolutionRevitAPI.WPF.ViewModels;
using SolutionRevitAPI.WPF.Views;
using System;
using System.Linq;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class EditSheetCommand : IExternalCommand
    {
        private static ExternalEvent _externalEvent;
        private static EditSheetEEH _handler;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Tạo và cấu hình handler
                _handler = new EditSheetEEH();
                _externalEvent = ExternalEvent.Create(_handler);

                EditSheet window = new EditSheet();
                EditSheetVM viewModel = new EditSheetVM() { Doc = doc, ExternalEvent = _externalEvent, Handler = _handler };
                var lstView = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType()
                                .ToElements().Cast<View>().Where(p => p.CanBePrinted == true).ToList();
                foreach (var item in lstView)
                {
                    viewModel.LstView.Add(item);
                }
                var lstSheet = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets).WhereElementIsNotElementType()
                                .ToElements().Cast<ViewSheet>().ToList();
                foreach (var item in lstSheet)
                {
                    viewModel.LstSheet.Add(item);
                }
                viewModel.TitleBlock = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_TitleBlocks)
                                        .WhereElementIsElementType().Cast<FamilySymbol>().ToList().FirstOrDefault();

                window.DataContext = viewModel;
                window.Topmost = true;
                window.Show();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}