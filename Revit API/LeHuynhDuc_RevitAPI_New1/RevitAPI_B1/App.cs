using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace RevitAPI_B1
{
    [Transaction(TransactionMode.Manual)]
    public class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //create ribbon
            String tabName = "___Demo1___";
            application.CreateRibbonTab(tabName);
            string path = Assembly.GetExecutingAssembly().Location;
            //BUTTON//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            PushButtonData btn34 = new PushButtonData("CmdToDuc1", "CmdToDuc1", path, "RevitAPI_B1.Commands.CmdToDuc1");
            //DG2///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            RibbonPanel panel12 = application.CreateRibbonPanel(tabName, "GD2");
            SplitButtonData sbd11 = new SplitButtonData("GD2", "GD2");
            SplitButton sb11 = panel12.AddItem(sbd11) as SplitButton;
            sb11.AddPushButton(btn34);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return Result.Succeeded;
        }
    }
}
