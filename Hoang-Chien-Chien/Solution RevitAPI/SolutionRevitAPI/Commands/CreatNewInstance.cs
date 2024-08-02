using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SolutionRevitAPI.WPF.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreatNewInstance : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            // Sử dụng phương thức PickPoint để lấy điểm người dùng click chuột
            XYZ clickedPoint;
            try
            {
                clickedPoint = uidoc.Selection.PickPoint("Chọn một điểm để tạo tường");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            try
            {
                FilteredElementCollector colector = new FilteredElementCollector(doc);
                ObservableCollection<Level> lstLevel = new ObservableCollection<Level>(colector.OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements().Cast<Level>().ToList());
                ObservableCollection<Category> lstCategory = new ObservableCollection<Category>();
                foreach (Category category in doc.Settings.Categories)
                {
                    if (category.BuiltInCategory == BuiltInCategory.OST_StructuralColumns || category.BuiltInCategory == BuiltInCategory.OST_Walls)
                    {
                        lstCategory.Add(category);
                    }
                }
                WPF.Views.CreatNewInstance window = new WPF.Views.CreatNewInstance();
                CreatNewInstanceVM viewModel = new CreatNewInstanceVM() { Uidoc = uidoc, Doc = doc, LstLevel = lstLevel, LstCategory = lstCategory };
                window.DataContext = viewModel;
                window.ShowDialog();
                if (viewModel.IsSave)
                {
                    using (Transaction trans = new Transaction(doc, "SetValue"))
                    {
                        trans.Start();
                        if (!viewModel.SelectedFamilySymbol.IsActive)
                        {
                            viewModel.SelectedFamilySymbol.Activate();
                        }
                        doc.Create.NewFamilyInstance(clickedPoint, viewModel.SelectedFamilySymbol, viewModel.SelectedLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        trans.Commit();
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