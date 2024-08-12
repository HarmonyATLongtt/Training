using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SolutionRevitAPI.WPF.ViewModels;
using SolutionRevitAPI.WPF.Views;
using System;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class DistanceBetweenGrids : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                SelectionFilter gridSelection = new SelectionFilter(BuiltInCategory.OST_Grids);
                Reference reference1 = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, gridSelection, "Chọn grid thứ nhất");
                bool checkSelect = true;
                Reference reference2 = null;
                while (checkSelect)
                {
                    reference2 = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, gridSelection, "Chọn grid thứ hai");
                    if (reference2 != null && doc.GetElement(reference1).Id != doc.GetElement(reference2).Id)
                    {
                        checkSelect = false;
                    }
                }
                if (reference1 != null && reference2 != null)
                {
                    Grid Grid1 = doc.GetElement(reference1) as Grid;
                    Grid Grid2 = doc.GetElement(reference2) as Grid;

                    Line line1 = Grid1.Curve as Line;
                    Line line2 = Grid2.Curve as Line;

                    // Lấy các vector chỉ phương
                    XYZ direction1 = line1.Direction;
                    XYZ direction2 = line2.Direction;
                    var Point1 = line1.GetEndPoint(1);
                    var Ponit2 = line2.GetEndPoint(1);
                    // Kiểm tra nếu các vector hướng có song song
                    bool isParallel = IsVectorsParallel(direction1, direction2);

                    if (isParallel)
                    {
                        EditDistanceGrids window = new EditDistanceGrids();
                        EditDistanceGridsVM viewModel = new EditDistanceGridsVM();
                        Double currentDistance = UnitUtils.ConvertFromInternalUnits(Point1.DistanceTo(Ponit2), UnitTypeId.Millimeters);
                        viewModel.Distance = currentDistance;
                        viewModel.Note = $"If the distance changes, the Grid {Grid2.Name} position will change. Unit: mm";
                        window.DataContext = viewModel;
                        window.ShowDialog();
                        if (viewModel.IsSave && currentDistance != viewModel.Distance)
                        {
                            //Set value
                            using (Transaction trans = new Transaction(doc, "SetValue"))
                            {
                                trans.Start();
                                XYZ moveVector = (Ponit2 - Point1).Normalize();
                                moveVector = moveVector.Multiply(UnitUtils.ConvertToInternalUnits(viewModel.Distance - currentDistance, UnitTypeId.Millimeters));
                                ElementTransformUtils.MoveElement(doc, doc.GetElement(reference2).Id, moveVector);
                                trans.Commit();
                            }
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Warning", "Hai grid không song song nên không tính được khoảng các giữa chúng");
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

        private bool IsVectorsParallel(XYZ direction1, XYZ direction2)
        {
            var result = direction1.CrossProduct(direction2);
            return result.IsZeroLength();
        }
    }
}