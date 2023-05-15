using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System.Reflection;

namespace ClassLibrary1
{
    [TransactionAttribute(TransactionMode.Manual)]
    internal class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //create tab
            string nameTab = "RibbonAdd";
            application.CreateRibbonTab(nameTab);

            //create panel
            RibbonPanel panel = application.CreateRibbonPanel(nameTab, "Commands");

            //create button
            string path = Assembly.GetExecutingAssembly().Location;
            PushButtonData button = new PushButtonData("btnButton1", "PlaceFamily", path, "ClassLibrary1.PlaceFamily");

            //add button to panel
            panel.AddItem(button);

            return Result.Succeeded;
        }
    }
}