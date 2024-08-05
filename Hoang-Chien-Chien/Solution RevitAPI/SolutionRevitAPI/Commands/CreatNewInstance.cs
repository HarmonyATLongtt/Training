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
                    if (category.BuiltInCategory == BuiltInCategory.OST_StructuralColumns || category.BuiltInCategory == BuiltInCategory.OST_Walls
                        || category.BuiltInCategory == BuiltInCategory.OST_StructuralFraming || category.BuiltInCategory == BuiltInCategory.OST_Floors || category.BuiltInCategory == BuiltInCategory.OST_Roofs)
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
                    using (Transaction trans = new Transaction(doc, "Creat New Instance"))
                    {
                        trans.Start();
                        Category category = viewModel.SelectedCategory;
                        if (category.BuiltInCategory == BuiltInCategory.OST_Walls)
                        {
                            // Xác định các điểm để tạo tường
                            XYZ startPoint = new XYZ(0, 0, 0); // Điểm bắt đầu
                            XYZ endPoint = new XYZ(10, 0, 0);   // Điểm kết thúc

                            // Tạo một Line từ điểm bắt đầu và kết thúc
                            Line wallLine = Line.CreateBound(startPoint, endPoint);

                            // Tạo tường mới
                            if (viewModel.SelectedFamilySymbol is WallType wallType)
                            {
                                Wall newWall = Wall.Create(doc, wallLine, wallType.Id, viewModel.SelectedLevel.Id, UnitUtils.ConvertToInternalUnits(4.0, UnitTypeId.Meters), 0.0, false, false);
                            }
                        }
                        else if (category.BuiltInCategory == BuiltInCategory.OST_Roofs)
                        {
                        }
                        else if (category.BuiltInCategory == BuiltInCategory.OST_Floors)
                        {
                        }
                        else
                        {
                            var familySymbol = viewModel.SelectedFamilySymbol as FamilySymbol;
                            if (!familySymbol.IsActive)
                            {
                                familySymbol.Activate();
                            }
                            doc.Create.NewFamilyInstance(clickedPoint, familySymbol, viewModel.SelectedLevel, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        }
                        trans.Commit();
                    }
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Người dùng hủy bỏ việc chọn đối tượng
                return Result.Cancelled; ;
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