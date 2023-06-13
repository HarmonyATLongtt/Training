#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls.Ribbon;

#endregion

namespace RebbonAdd
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            RibbonPanel ribbonPanel = null;

            // Tạo tab trên Ribbon (nếu chưa tồn tại)
            string tabName = "Custom Tab";
            RibbonTab ribbonTab = null;
            foreach (RibbonTab tab in uiApp.GetRibbonPanels())
            {
                if (tab.Name == tabName)
                {
                    ribbonTab = tab;
                    break;
                }
            }
            if (ribbonTab == null)
            {
                ribbonTab = uiApp.CreateRibbonTab(tabName);
            }

            // Tạo panel trên tab (nếu chưa tồn tại)
            string panelName = "Custom Panel";
            List<RibbonPanel> panels = ribbonTab.GetItems()
                                                .OfType<RibbonPanel>()
                                                .Where(p => p.Name == panelName)
                                                .ToList();
            if (panels.Count == 0)
            {
                ribbonPanel = ribbonTab.AddItem(new RibbonPanelData(panelName, panelName)) as RibbonPanel;
            }
            else
            {
                ribbonPanel = panels[0];
            }

            // Thêm nút vào panel
            PushButtonData buttonData1 = new PushButtonData("button1", "Button 1", "path_to_icon1");
            PushButton pushButton1 = ribbonPanel.AddItem(buttonData1) as PushButton;

            PushButtonData buttonData2 = new PushButtonData("button2", "Button 2", "path_to_icon2");
            PushButton pushButton2 = ribbonPanel.AddItem(buttonData2) as PushButton;

            // Hiển thị Ribbon
            uidoc.RefreshActiveView();

            return Result.Succeeded;
        }
    }
}
