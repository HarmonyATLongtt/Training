using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace RevitTrainees
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
            string asseemblyPath = Assembly.GetExecutingAssembly().Location;

            RibbonPanel panel = RibbonPanel(application, "Trainees");

            SplitButtonData splitButtonData = new SplitButtonData("Split exercise 1", "Exercise 1");
            SplitButton sbExer1 = panel.AddItem(splitButtonData) as SplitButton;

            #region Create Column Button

            PushButtonData btnColumn = new PushButtonData("Create Column", "Column", asseemblyPath, "RevitTrainees.Commands.Cmd_CreateColumn");
            btnColumn.ToolTip = "Create column in (0, 0, 0) location...";
            Uri uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "Column.ico"));
            BitmapImage bitmap = new BitmapImage(uri);
            btnColumn.LargeImage = bitmap;
            sbExer1.AddPushButton(btnColumn);

            #endregion Create Column Button

            #region Create Wall Button

            PushButtonData btnWall = new PushButtonData("Create Wall", "Wall", asseemblyPath, "RevitTrainees.Commands.Cmd_CreateWall");
            btnWall.ToolTip = "Create new wall...";
            uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "Wall.ico"));
            bitmap = new BitmapImage(uri);
            btnWall.LargeImage = bitmap;
            sbExer1.AddPushButton(btnWall);

            #endregion Create Wall Button

            #region Create Beam Button

            PushButtonData btnBeam = new PushButtonData("Create Beam", "Beam", asseemblyPath, "RevitTrainees.Commands.Cmd_CreateBeam");
            btnBeam.ToolTip = "Create a new beam...";
            uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "Beam.ico"));
            bitmap = new BitmapImage(uri);
            btnBeam.LargeImage = bitmap;
            sbExer1.AddPushButton(btnBeam);

            #endregion Create Beam Button

            #region Create Floor Button

            PushButtonData btnFloor = new PushButtonData("Create Floor", "Floor", asseemblyPath, "RevitTrainees.Commands.Cmd_CreateFloor");
            btnFloor.ToolTip = "Create new floor in active view...";
            uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "Floor.ico"));
            bitmap = new BitmapImage(uri);
            btnFloor.LargeImage = bitmap;
            sbExer1.AddPushButton(btnFloor);

            #endregion Create Floor Button

            #region Create Room Button

            PushButtonData btnRoom = new PushButtonData("Create Room", "Room", asseemblyPath, "RevitTrainees.Commands.Cmd_CreateRoom");
            btnRoom.ToolTip = "Create new room in active view...";
            uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "Room.ico"));
            bitmap = new BitmapImage(uri);
            btnRoom.LargeImage = bitmap;
            sbExer1.AddPushButton(btnRoom);

            #endregion Create Room Button

            panel.AddSeparator();

            SplitButtonData sbDataFamily = new SplitButtonData("Split exercise 2", "Exercise 2");
            SplitButton sbExer2 = panel.AddItem(sbDataFamily) as SplitButton;

            #region Create Family Button

            PushButtonData btnCreateFamily = new PushButtonData("Create Family", "Family Extrusion", asseemblyPath, "RevitTrainees.Commands.Cmd_CreateFamily");
            btnCreateFamily.ToolTip = "Creeate new family is a extrusion block...";
            uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "CreateFamily.ico"));
            bitmap = new BitmapImage(uri);
            btnCreateFamily.LargeImage = bitmap;
            sbExer2.AddPushButton(btnCreateFamily);

            #endregion Create Family Button

            #region Create Load And Place Instance Family Button

            PushButtonData btnLoadFamily = new PushButtonData("Load Family", "Load Family", asseemblyPath, "RevitTrainees.Commands.Cmd_LoadFamily");
            btnLoadFamily.ToolTip = "Load a family and place an instance of it in place you choose...";
            uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "LoadFamily.ico"));
            bitmap = new BitmapImage(uri);
            btnLoadFamily.LargeImage = bitmap;
            sbExer2.AddPushButton(btnLoadFamily);

            #endregion Create Load And Place Instance Family Button

            #region Create Edit And Reload Family Button

            PushButtonData btnEditFamily = new PushButtonData("Edit Family", "Edit Family", asseemblyPath, "RevitTrainees.Commands.Cmd_EditAndReloadFamily");
            btnEditFamily.ToolTip = "Edit family when pick element and reload...";
            uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "EditFamily.ico"));
            bitmap = new BitmapImage(uri);
            btnEditFamily.LargeImage = bitmap;
            sbExer2.AddPushButton(btnEditFamily);

            #endregion Create Edit And Reload Family Button

            panel.AddSeparator();

            #region Creeate Filter Wall Button

            if (panel.AddItem(new PushButtonData("Filter", "Filter Wall", asseemblyPath, "RevitTrainees.Commands.Cmd_FilterWall")) is PushButton btnFilter)
            {
                btnFilter.ToolTip = "Create Fillter For Wall By Area...";
                uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "FilterWall.ico"));
                bitmap = new BitmapImage(uri);
                btnFilter.LargeImage = bitmap;
            }

            #endregion Creeate Filter Wall Button

            panel.AddSeparator();

            #region Create New Instance In Level Button

            if (panel.AddItem(new PushButtonData("Load Level", "Instance In Level", asseemblyPath, "RevitTrainees.Commands.Cmd_InstanceInLevel")) is PushButton btnLevel)
            {
                btnLevel.ToolTip = "Create an instance of column in each view...";
                uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "Level.ico"));
                bitmap = new BitmapImage(uri);
                btnLevel.LargeImage = bitmap;
            }

            #endregion Create New Instance In Level Button

            panel.AddSeparator();

            #region Create Edit Grid Button

            if (panel.AddItem(new PushButtonData("Edit Grid", "Edit Grid", asseemblyPath, "RevitTrainees.Commands.Cmd_EditGrid")) is PushButton btnEditGrid)
            {
                btnEditGrid.ToolTip = "Calculate distance between two grid paralel and change distance of it...";
                uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "EditGrid.ico"));
                bitmap = new BitmapImage(uri);
                btnEditGrid.LargeImage = bitmap;
            }

            #endregion Create Edit Grid Button

            panel.AddSeparator();

            SplitButtonData sbAnnotation = new SplitButtonData("Split exercise 3", "Exercise 3");
            SplitButton sbExer3 = panel.AddItem(sbAnnotation) as SplitButton;

            #region Create Dimension For Object Button

            PushButtonData btnDimension = new PushButtonData("Create Dimension", "Create Dimension", asseemblyPath, "RevitTrainees.Commands.Cmd_CreateDimension");
            btnDimension.ToolTip = "Create new dimension between two object...";
            uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "Dimension.ico"));
            bitmap = new BitmapImage(uri);
            btnDimension.LargeImage = bitmap;
            sbExer3.AddPushButton(btnDimension);

            #endregion Create Dimension For Object Button

            #region Create Text Button

            PushButtonData btnText = new PushButtonData("Create Text", "Create Text", asseemblyPath, "RevitTrainees.Commands.Cmd_CreateText");
            btnText.ToolTip = "Create new text in specific location...";
            uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "Text.ico"));
            bitmap = new BitmapImage(uri);
            btnText.LargeImage = bitmap;
            sbExer3.AddPushButton(btnText);

            #endregion Create Text Button

            #region Create Tag Button

            PushButtonData btnTag = new PushButtonData("Create Tag", "Create Tag", asseemblyPath, "RevitTrainees.Commands.Cmd_CreateTag");
            btnTag.ToolTip = "Create new tag for object selected...";
            uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "Tag.ico"));
            bitmap = new BitmapImage(uri);
            btnTag.LargeImage = bitmap;
            sbExer3.AddPushButton(btnTag);

            #endregion Create Tag Button

            panel.AddSeparator();

            #region Creeate Schedule Button

            if (panel.AddItem(new PushButtonData("Schedule", "Schedule Wall", asseemblyPath, "RevitTrainees.Commands.Cmd_CreateSchedule")) is PushButton btnSchedule)
            {
                btnSchedule.ToolTip = "Create New Schedule for Walls...";
                uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "Schedule.ico"));
                bitmap = new BitmapImage(uri);
                btnSchedule.LargeImage = bitmap;
            }

            #endregion Creeate Schedule Button

            panel.AddSeparator();

            #region Creeate Sheet Button

            if (panel.AddItem(new PushButtonData("Sheet", "Sheet", asseemblyPath, "RevitTrainees.Commands.Cmd_CreateSheet")) is PushButton btnSheet)
            {
                btnSheet.ToolTip = "Create New Sheet...";
                uri = new Uri(Path.Combine(Path.GetDirectoryName(asseemblyPath), "Icons", "Sheet.ico"));
                bitmap = new BitmapImage(uri);
                btnSheet.LargeImage = bitmap;
            }

            #endregion Creeate Sheet Button

            return Result.Succeeded;
        }

        protected RibbonPanel RibbonPanel(UIControlledApplication app, string panelName)
        {
            string tabName = "Trainees";
            app.CreateRibbonTab(tabName);

            app.CreateRibbonPanel(tabName, panelName);

            RibbonPanel ribbonPanel = null;
            ribbonPanel = app.GetRibbonPanels(tabName).FirstOrDefault(e => e.Name.Equals(panelName));
            return ribbonPanel;
        }
    }
}