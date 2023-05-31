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
    [TransactionAttribute(TransactionMode.Manual)]  // giúp revit hiểu cách đọc lệnh này như thế nào
    internal class CreateViewPlan : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // get uidocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            // get document
            Document doc = uidoc.Document;

            // get level 1
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            Level level = collector.OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .First(x => x.Name == "Level 1");
            ViewFamilyType viewFamily = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .First(x => x.ViewFamily == ViewFamily.FloorPlan);
            try
            {
                using (Transaction trans = new Transaction(doc, "create view plan"))
                {
                    trans.Start();

                    ViewPlan viewPlan = ViewPlan.Create(doc, viewFamily.Id, level.Id);
                    viewPlan.Name = "ViewPlan using Visual Studio";

                    trans.Commit();
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