using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class DeleteElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //Get Document
            Document doc = uidoc.Document;

            try
            {
                //Get reference of Element
                Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (reference != null)
                {
                    Element element = doc.GetElement(reference);
                    using (Transaction trans = new Transaction(doc, "Change Element"))
                    {
                        trans.Start();
                        doc.Delete(element.Id);
                        using (TaskDialog dialog = new TaskDialog("Warning"))
                        {
                            dialog.MainContent = "Bạn có muốn xóa đối tượng này không";
                            dialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                            if (dialog.Show() == TaskDialogResult.Yes)
                            {
                                TaskDialog.Show("Information", "Xóa đối tượng thành công");
                                trans.Commit();
                            }
                            else
                                trans.RollBack();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}