using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Bai2
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
    internal class HelloWorld : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Thong bao", "Hello World");
            return Result.Succeeded;
        }
    }
}
