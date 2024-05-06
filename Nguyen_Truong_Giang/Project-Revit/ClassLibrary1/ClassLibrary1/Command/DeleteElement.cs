using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace ClassLibrary1
{
    [TransactionAttribute(TransactionMode.Manual)]
    class DeleteElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get Document
            Document doc = uidoc.Document;

            try
            {
                //get Reference Element
                Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);

                if (r != null)
                {
                    using (Transaction trans = new Transaction(doc, "Delete Element"))
                    {
                        trans.Start();

                        doc.Delete(r.ElementId);

                        TaskDialog tDialog = new TaskDialog("Delete Element");
                        tDialog.MainContent = "Do you want delete this Elemet?";
                        tDialog.CommonButtons = TaskDialogCommonButtons.Ok | TaskDialogCommonButtons.Cancel;

                        if(tDialog.Show() == TaskDialogResult.Ok)
                        {
                            trans.Commit();
                            TaskDialog.Show("Delete Element", "Delete Element: " + r.ElementId.ToString() + " Success");
                        } else
                        {
                            trans.RollBack();
                            TaskDialog.Show("Delete Element", "Delete Element: " + r.ElementId.ToString() + " Cancel");
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
