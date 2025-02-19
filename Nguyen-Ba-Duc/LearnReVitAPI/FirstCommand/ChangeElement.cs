using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class ChangeElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get UIdocument
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                // Get Referenct Element
                Reference r = uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (r != null)
                {
                    using (Transaction tr = new Transaction(doc, "Change Element"))
                    {
                        tr.Start();
                        doc.Delete(r.ElementId);

                        TaskDialog taskDialog = new TaskDialog("Delete Element");
                        taskDialog.MainContent = "Are you sure to delete this element?";
                        taskDialog.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;

                        if (taskDialog.Show() == TaskDialogResult.Ok)
                        {
                            tr.Commit();
                            TaskDialog.Show("Delete element", "You deleted element " + r.ElementId.ToString());
                        }
                        else
                        {
                            tr.RollBack();
                            TaskDialog.Show("Delete element", "The element " + r.ElementId.ToString() + " has not been deleted");
                        }
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}