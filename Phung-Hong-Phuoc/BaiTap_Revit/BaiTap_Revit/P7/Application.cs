using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace BaiTap_Revit.P7
{
    internal class Application : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            RibbonPanel panel = RibbonPanel(application);
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            if (panel.AddItem(new PushButtonData("MyPlugin", "MyPlugin", thisAssemblyPath, "BaiTap_Revit.P7.LoadLevel"))
               is PushButton button)
            {
                button.ToolTip = "My Plugin";
                
            }

            return Result.Succeeded;
        }

        public RibbonPanel RibbonPanel(UIControlledApplication app)
        {
            string tab = "Hong Phuoc";
            RibbonPanel ribbonPanel = null;

            try
            {
                app.CreateRibbonTab(tab);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            try
            {
                app.CreateRibbonPanel(tab, "Phuocdz");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            List<RibbonPanel> panels = app.GetRibbonPanels(tab);
            foreach (RibbonPanel panel in panels.Where(x => x.Name == "Phuocdz"))
            {
                ribbonPanel = panel;
            }
            return ribbonPanel;
        }
    }
}