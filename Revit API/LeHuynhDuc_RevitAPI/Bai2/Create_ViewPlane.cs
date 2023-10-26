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
    internal class Create_ViewPlane : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            ViewFamilyType viewFamilyType = new FilteredElementCollector(doc)
                                            .OfClass(typeof(ViewFamilyType))
                                            .Cast<ViewFamilyType>()
                                            .First(x => x.ViewFamily == ViewFamily.FloorPlan);

            Level level = new FilteredElementCollector(doc)
                            .OfClass(typeof(Level))
                            .OfCategory(BuiltInCategory.OST_Levels)
                            .WhereElementIsNotElementType()
                            .Cast<Level>()
                            .First(x => x.Name == "Level 1");
            using(Transaction trans = new Transaction(doc, "create_ViewPlane"))
            {
                trans.Start();
                ViewPlan viewPlan =  ViewPlan.Create(doc, viewFamilyType.Id, level.Id);
                //viewPlan.Name = "New level";
                trans.Commit(); 
            }
            return Result.Succeeded;
        }
    }
}
