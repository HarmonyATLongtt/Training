using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SolutionRevitAPI.WPF.ViewModels;
using SolutionRevitAPI.WPF.Views;
using System;
using System.Linq;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class CreatFilterForView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                EditFilterForView window = new EditFilterForView();
                EditFilterForViewVM viewModel = new EditFilterForViewVM() { Doc = doc };
                var lstView = new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType()
                                .ToElements().Cast<View>().Where(p => p.CanBePrinted == true).ToList();
                foreach (var item in lstView)
                {
                    viewModel.LstView.Add(item);
                }
                viewModel.CurrentWindow = window;
                window.DataContext = viewModel;
                window.ShowDialog();
                foreach (var item in viewModel.LstView)
                {
                    foreach (var filter in item.GetFilters())
                    {

                    }
                }
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