using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using Autodesk.Revit.UI.Selection;

namespace BaiTap_Revit.P1
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class DeleteElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (reference != null)
                {
                    Element element = doc.GetElement(reference);
                    using (Transaction trans = new Transaction(doc, "Change Element"))
                    {
                        trans.Start();

                        using (TaskDialog dialog = new TaskDialog("Warning"))
                        {
                            dialog.MainContent = "Bạn có muốn xóa đối tượng này không";
                            dialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                            if (dialog.Show() == TaskDialogResult.Yes)
                            {
                                doc.Delete(element.Id);
                                trans.Commit();
                                TaskDialog.Show("Information", "Xóa đối tượng thành công");
                                
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