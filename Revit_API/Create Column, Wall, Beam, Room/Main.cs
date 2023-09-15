using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Windows.Media.Imaging;

namespace CreateColumnApi
{
    public class Main : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // Get path of *.dll file
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            string mainDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Create a panel name 'Create'
            RibbonPanel panel = RibbonPanel(application, "Create");

            // Create Column button
            if (panel.AddItem(new PushButtonData("Create Column", "Column", assemblyPath, "CreateColumnApi.ColumnCommand")) is PushButton columnButton)
            {
                Uri uri = new Uri(@"E:\Intern App\Revit_API\CreateColumnApi\Icons\Column.png");
                BitmapImage largeImage = new BitmapImage(uri);
                columnButton.LargeImage = largeImage;
                columnButton.Image = largeImage;
                columnButton.ToolTip = "Create a column architecture in your point you choose!";
            }
            // End create column button

            // Add a separator
            panel.AddSeparator();

            // Create wall button
            if (panel.AddItem(new PushButtonData("Create Wall", "Wall", assemblyPath, "CreateColumnApi.WallCommand")) is PushButton wallButton)
            {
                Uri uri = new Uri(@"E:\Intern App\Revit_API\CreateColumnApi\Icons\Wall.png");
                BitmapImage largeImage = new BitmapImage(uri);
                wallButton.LargeImage = largeImage;
                wallButton.Image = largeImage;
                wallButton.ToolTip = "Create a wall with two point...";
            }
            // End create wall button

            // Add a separator
            panel.AddSeparator();

            // Create beam button
            if (panel.AddItem(new PushButtonData("Create Beam", "Beam", assemblyPath, "CreateColumnApi.BeamsCommand")) is PushButton beamButton)
            {
                Uri uri = new Uri(@"E:\Intern App\Revit_API\CreateColumnApi\Icons\Beams.png");
                BitmapImage image = new BitmapImage(uri);
                beamButton.Image = image;
                beamButton.LargeImage = image;
                beamButton.ToolTip = "Create a beams with two point pick...";
            }
            // End create beam button

            panel.AddSeparator();

            // Create floor button
            if (panel.AddItem(new PushButtonData("Create Floor", "  Floor  ", assemblyPath, "CreateColumnApi.FloorCommand")) is PushButton floorButton)
            {
                Uri uri = new Uri(@"E:\Intern App\Revit_API\CreateColumnApi\Icons\Floor.png");
                BitmapImage image = new BitmapImage(uri);
                floorButton.Image = image;
                floorButton.LargeImage = image;
                floorButton.ToolTip = "Create a floor automatically... ";
            }
            // End create floor button

            panel.AddSeparator();

            // Create room button
            if (panel.AddItem(new PushButtonData("Create Room", "   Room   ", assemblyPath, "CreateColumnApi.RoomCommand")) is PushButton roomButton)
            {
                Uri uri = new Uri(@"E:\Intern App\Revit_API\CreateColumnApi\Icons\Room.png");
                BitmapImage image = new BitmapImage(uri);
                roomButton.Image = image;
                roomButton.LargeImage = image;
                roomButton.ToolTip = "Create a floor automatically... ";
            }

            panel.AddSeparator();

            if(panel.AddItem(new PushButtonData("Select Element", "Get Information", assemblyPath, "CreateColumnApi.SelectCommand")) is PushButton selectButton)
            {
                Uri uri = new Uri(@"D:\Training\Revit_API\Create Column, Wall, Beam, Room\Icons\Infor.png");
                BitmapImage image = new BitmapImage(uri);
                selectButton.Image = image;
                selectButton.ToolTip = "Select an element and get its information...";
            }

            panel.AddSeparator();
            if (panel.AddItem(new PushButtonData("Set Comments", "Set Comments", assemblyPath, "CreateColumnApi.SetCommentsCommand")) is PushButton setCommentsButton)
            {
                Uri uri = new Uri(@"D:\Training\Revit_API\Create Column, Wall, Beam, Room\Icons\Set.png");
                BitmapImage image = new BitmapImage(uri);
                setCommentsButton.Image = image;
                setCommentsButton.ToolTip = "Select an element and set its comments...";
            }

            return Result.Succeeded;
        }

        public RibbonPanel RibbonPanel(UIControlledApplication application, string panelName)
        {
            string tabName = "Chinsu";
            application.CreateRibbonTab(tabName);

            application.CreateRibbonPanel(tabName, panelName);

            RibbonPanel ribbonPanel = null;

            ribbonPanel = application.GetRibbonPanels(tabName).FirstOrDefault(x => x.Name.Equals(panelName));

            return ribbonPanel;
        }
    }
}
