using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Document = Autodesk.Revit.DB.Document;

namespace Bai2
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    internal class Create_Wall : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            FilteredElementCollector collector = new FilteredElementCollector(doc);
            Level level = collector.OfCategory(BuiltInCategory.OST_Levels)
                          .WhereElementIsNotElementType()
                          .Cast<Level>()
                          .First(x => x.Name == "Level 1");
            XYZ p1 = new XYZ(0, 0, 0);
            XYZ p2 = new XYZ(10, 0, 0);

            Line line = Line.CreateBound(p1, p2);

            try
            {
                using (Transaction trans = new Transaction(doc, "create_Wall"))
                {
                    trans.Start();
                    Wall wall = Wall.Create(doc, line, level.Id, false);
                    
                    Parameter heightParameter = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                    heightParameter.Set(10.0);
                   // wall.Name = "My wall";
                    trans.Commit();
                }
                return Result.Succeeded;
            }
            catch(Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
