using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Linq;

namespace SolutionRevitAPI.Commands
{
    [Transaction(TransactionMode.Manual)]
    internal class CreatViewPlan : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Get UIDocumnent
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                Level level = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToElements().Cast<Level>().Where(p => p.Name == "Level 3").FirstOrDefault();
                ViewFamilyType viewFamily = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).WhereElementIsElementType().Cast<ViewFamilyType>().Where(p => p.ViewFamily == ViewFamily.StructuralPlan).FirstOrDefault();
                using (Transaction trans = new Transaction(doc, "Creat View Plan"))
                {
                    trans.Start();
                    ViewPlan viewPlan = ViewPlan.Create(doc, viewFamily.Id, level.Id);
                    viewPlan.Name = "ABCD";
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
}