using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SolutionRevitAPI.Commands.ExternalEventHandler;
using SolutionRevitAPI.WPF.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreatNewInstance2 : IExternalCommand
    {
        private static ExternalEvent _externalEvent;
        private static CreatNewInstanceEEH _handler;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                FilteredElementCollector colector = new FilteredElementCollector(doc);
                ObservableCollection<Level> lstLevel = new ObservableCollection<Level>(colector.OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements().Cast<Level>().ToList());
                ObservableCollection<Category> lstCategory = new ObservableCollection<Category>();
                foreach (Category category in doc.Settings.Categories)
                {
                    if (category.BuiltInCategory == BuiltInCategory.OST_StructuralColumns || category.BuiltInCategory == BuiltInCategory.OST_Walls
                        || category.BuiltInCategory == BuiltInCategory.OST_StructuralFraming || category.BuiltInCategory == BuiltInCategory.OST_Floors || category.BuiltInCategory == BuiltInCategory.OST_Roofs)
                    {
                        lstCategory.Add(category);
                    }
                }
                // Tạo và cấu hình handler
                _handler = new CreatNewInstanceEEH();
                _externalEvent = ExternalEvent.Create(_handler);
                WPF.Views.CreatNewInstance window = new WPF.Views.CreatNewInstance();
                CreatNewInstanceVM viewModel = new CreatNewInstanceVM() { Uidoc = uidoc, Doc = doc, ExternalEvent = _externalEvent, Handler = _handler, LstLevel = lstLevel, LstCategory = lstCategory };
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