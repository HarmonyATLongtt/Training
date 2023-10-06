using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;

namespace RevitTrainees.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            Document doc = uiDoc.Document;

            PlaceAlignedViewsAtLeftCorner(doc);

            return Result.Succeeded;
        }

        public static void PlaceAlignedViewsAtLeftCorner(Document doc)
        {
            FilteredElementCollector fec = new FilteredElementCollector(doc);
            fec.OfClass(typeof(ViewPlan));

            var viewPlans = fec.Cast<ViewPlan>().Where(vp => !vp.IsTemplate && vp.ViewType == ViewType.FloorPlan);
            ViewPlan vp1 = viewPlans.ElementAt(0);
            ViewPlan vp2 = viewPlans.ElementAt(1);

            try
            {
                using (var trans = new Transaction(doc, "Place on sheet"))
                {
                    trans.Start();

                    // Add two viewport distinct from one another
                    ViewSheet vs = ViewSheet.Create(doc, ElementId.InvalidElementId);

                    Viewport viewport1 = Viewport.Create(doc, vs.Id, vp1.Id, new XYZ(0, 0, 0));
                    Viewport viewport2 = Viewport.Create(doc, vs.Id, vp2.Id, new XYZ(0, 5, 0));

                    doc.Regenerate();

                    Outline outline1 = viewport1.GetBoxOutline();
                    Outline outline2 = viewport2.GetBoxOutline();
                    XYZ boxCenter = viewport2.GetBoxCenter();
                    XYZ vectorToCenter = boxCenter - outline2.MinimumPoint;
                    XYZ newcenter = outline1.MinimumPoint + vectorToCenter;

                    viewport2.SetBoxCenter(newcenter);

                    trans.Commit();
                }
            }
            catch { }
        }
    }
}