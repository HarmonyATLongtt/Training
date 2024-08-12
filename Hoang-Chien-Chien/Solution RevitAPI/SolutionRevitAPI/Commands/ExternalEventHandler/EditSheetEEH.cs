using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;

namespace SolutionRevitAPI.Commands.ExternalEvents
{
    public class EditSheetEEH : IExternalEventHandler
    {
        public FamilySymbol TitleBlock { get; set; }
        public string SheetName { get; set; }
        public string SheetNumber { get; set; }
        public View SelectedViewPlan { get; set; }
        public ViewSheet SelectedSheet { get; set; }

        public ObservableCollection<ViewSheet> LstSheet;
        public int Mode { get; set; }

        public void Execute(UIApplication app)
        {
            UIDocument uidoc = app.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Mode == 1)
            {
                using (Transaction trans = new Transaction(doc, "Create Sheet"))
                {
                    trans.Start();
                    ViewSheet viewSheet = ViewSheet.Create(doc, TitleBlock.Id);
                    viewSheet.Name = SheetName;
                    viewSheet.SheetNumber = SheetNumber;
                    trans.Commit();
                    LstSheet.Add(viewSheet);
                }
            }
            if (Mode == 2)
            {
                using (Transaction trans = new Transaction(doc, "Set View To Sheet"))
                {
                    BoundingBoxUV uv = SelectedSheet.Outline;
                    double ux = (uv.Max.U + uv.Min.U) / 2;
                    double uy = (uv.Max.V + uv.Min.V) / 2;
                    XYZ point = new XYZ(ux, uy, 0);
                    trans.Start();
                    Viewport viewport = Viewport.Create(doc, SelectedSheet.Id, SelectedViewPlan.Id, point);
                    trans.Commit();
                }
            }
        }

        public string GetName()
        {
            return "Edit Sheet - View Plan";
        }
    }
}