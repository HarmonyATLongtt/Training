#region Namespaces

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

#endregion

namespace RibbonAdd
{
    [Transaction(TransactionMode.Manual)]
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            //Create Tab
            string nameTab = "Ribbon Add";
            a.CreateRibbonTab(nameTab);

            //Create Ribbon
            RibbonPanel panel = a.CreateRibbonPanel(nameTab, "Command");

            //Create button
            string path = Assembly.GetExecutingAssembly().Location;
            PushButtonData button = new PushButtonData("button1", "Place Columns", path, "RibbonAdd");

            //Add button to panel
            PushButton btn = panel.AddItem(button) as PushButton;

            //Add Icon to Button
            Uri UriSource = new Uri(@"C:\Users\giang\OneDrive\Máy tính");
            BitmapImage image = new BitmapImage(UriSource);
            btn.LargeImage = image;

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }
}