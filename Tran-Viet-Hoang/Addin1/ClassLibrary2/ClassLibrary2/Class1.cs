using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;


namespace ClassLibrary2
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //get uidoc
            //UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //getelement
            //Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            TaskDialog.Show("Chào ngày cũ", "Chào ngày siêu mới");
            return Result.Succeeded;
            //try
            //{
            //    Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element);
            //    if (r != null)
            //    {                 
            //        TaskDialog.Show("Element Id", r.ElementId.ToString());                  
            //    }
            //    return Result.Succeeded;
            //}
            //catch (Exception ex)
            //{
            //    message = ex.Message;
            //    return Result.Failed;
            //}


        }
    }
}
