using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bai2
{
    [Transaction(TransactionMode.Manual)]
    public class Create_Schedule : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            using(Transaction trans = new Transaction(doc, "Creater_Schedule"))
            {
                trans.Start();
                // Create schedule
                ViewSchedule vs = ViewSchedule.CreateSchedule(doc, new ElementId(BuiltInCategory.OST_Windows));
                vs.Name = "Schedule1";
                doc.Regenerate();

                trans.Commit();
            }
            return Result.Succeeded;
        }
    }
}
