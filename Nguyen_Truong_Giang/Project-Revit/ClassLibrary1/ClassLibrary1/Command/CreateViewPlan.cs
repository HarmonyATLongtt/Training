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
    class CreateViewPlan : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //get UIDocument
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            //get Document
            Document doc = uidoc.Document;

            //get Level 1
            FilteredElementCollector collecter = new FilteredElementCollector(doc);
            Level level = collecter.OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .First(x => x.Name == "Level 1");

            //get Family
            ViewFamilyType viewFamily = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .First(x => x.ViewFamily == ViewFamily.FloorPlan);

            try
            {
                using (Transaction trans = new Transaction(doc, "Create View Plan"))
                {
                    trans.Start();

                    ViewPlan viewplan = ViewPlan.Create(doc, viewFamily.Id, level.Id);
                    viewplan.Name = "My Floor Plan API";

                    trans.Commit();
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
