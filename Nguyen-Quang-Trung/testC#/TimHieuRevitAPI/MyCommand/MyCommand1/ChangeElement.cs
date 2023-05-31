using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace MyCommand1
{
    [TransactionAttribute(TransactionMode.Manual)] // giúp revit hiểu cách đọc lệnh này như thế nào
    internal class ChangeElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // get uidocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // get document
            Document doc = uidoc.Document;
            try
            {
                // get reference element
                Reference refer = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (refer != null)
                {
                    using (Transaction trans = new Transaction(doc, "Change Element"))
                    {
                        trans.Start();
                        doc.Delete(refer.ElementId);

                        TaskDialog notification = new TaskDialog("Delete Element");
                        notification.MainContent = "Ban co muon xoa phan tu nay khong";
                        notification.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;

                        if (notification.Show() == TaskDialogResult.Ok)
                        {
                            trans.Commit();
                            TaskDialog.Show("Delete Element", "Ban da xoa phan tu " + refer.ElementId.ToString());
                        }
                        else
                        {
                            trans.RollBack();
                            TaskDialog.Show("Delete Element", "Ban chua xoa phan tu " + refer.ElementId.ToString() + " . Neu muon xoa hay thu lai");
                        }
                    }
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}