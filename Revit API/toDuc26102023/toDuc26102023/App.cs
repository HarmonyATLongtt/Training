using Autodesk.Revit.UI;
using System;
using System.Reflection;

namespace toDuc26102023
{
    public class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //create ribbon
            String tabName = "__DucLe__";
            application.CreateRibbonTab(tabName);
            string path = Assembly.GetExecutingAssembly().Location;
            //BUTTON//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            PushButtonData btn1 = new PushButtonData("Cmd_ToDuc_26102023", "Cmd_ToDuc_26102023", path, "toDuc26102023.Commands.Cmd_ToDuc_26102023");
            //DG2///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            RibbonPanel panel1 = application.CreateRibbonPanel(tabName, "TamGiac");
            SplitButtonData sbd1 = new SplitButtonData("TamGiac", "TamGiac");
            SplitButton sb1 = panel1.AddItem(sbd1) as SplitButton;
            sb1.AddPushButton(btn1);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return Result.Succeeded;
        }
    }
}
