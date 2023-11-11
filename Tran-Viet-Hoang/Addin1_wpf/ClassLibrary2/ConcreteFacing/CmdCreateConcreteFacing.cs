using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using ConcreteFacing.Process;
using ConcreteFacing.Process.ObjectCoverSet;
using ConcreteFacing.UI.ViewModel;
using ConcreteFacing.UI.Views;
using System.Windows;

namespace ConcreteFacing
{
    [TransactionAttribute(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CmdCreateConcreteFacing : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var elems = new ObjectTypeCondition().PickConcreteBeamOrColumn(uidoc);
            if (elems != null && elems.Count > 0)
            {
                MainView view = new MainView();
                view.DataContext = new MainViewModel(elems);
                new DirectShapeSet().CreateDirectShapes(doc, view, elems);
            }
            else
            {
                MessageBox.Show("No element is selected!");
            }
            return Result.Succeeded;
        }
    }
}