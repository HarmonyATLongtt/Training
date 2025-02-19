using System;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace FirstCommand
{
    public class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                //Create tab
                string nameTab = "Duc Tab";
                application.CreateRibbonTab(nameTab);

                string panelName = "My Panel";
                RibbonPanel ribbonPanel = application.CreateRibbonPanel(nameTab, panelName);

                // Thêm nút vào panel
                string assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string path = System.IO.Path.GetDirectoryName(assemblyPath);

                PushButtonData buttonData1 = new PushButtonData(
                    "Place Family", // Internal Name
                    "Place Family",    // Displayed Name
                    assemblyPath,
                    "FirstCommand.PlaceFamily" // Full class name
                );
                //PushButtonData buttonData2 = new PushButtonData("TestWpf", "TestWpf", assemblyPath, "FirstCommand.TestWpf");
                PushButtonData buttonData2 = new PushButtonData("Get Face", "Get Face n Edge", assemblyPath, "FirstCommand.GetFace");
                PushButtonData buttonData3 = new PushButtonData("Collection Doors", "Door", assemblyPath, "FirstCommand.CollectionDoors");
                PushButtonData buttonData4 = new PushButtonData("Get Geometry", "Geometry", assemblyPath, "FirstCommand.GetGeometry");
                PushButtonData buttonData5 = new PushButtonData("Create Floor", "Floor", assemblyPath, "FirstCommand.CreateFloor");
                PushButtonData buttonData6 = new PushButtonData("Create Wall", "Wall", assemblyPath, "FirstCommand.CreateWall");
                PushButtonData buttonData7 = new PushButtonData("Create Column", "Column", assemblyPath, "FirstCommand.CreateColumn");
                PushButtonData buttonData8 = new PushButtonData("Create Beam", "Beam", assemblyPath, "FirstCommand.CreateBeam");
                PushButtonData buttonData9 = new PushButtonData("Create Room", "Room", assemblyPath, "FirstCommand.CreateRoom");
                PushButtonData buttonData10 = new PushButtonData("Create Family", "New Family", assemblyPath, "FirstCommand.CreateFamily");
                PushButtonData buttonData11 = new PushButtonData("Create Filter", "Filter", assemblyPath, "FirstCommand.CreateFilter");
                PushButtonData buttonData12 = new PushButtonData("Change Grid Distance", "Grid", assemblyPath, "FirstCommand.ChangeGridDistance");
                PushButtonData buttonData13 = new PushButtonData("Create Dimention", "Dimention", assemblyPath, "FirstCommand.CreateDim");
                PushButtonData buttonData14 = new PushButtonData("Create Tag", "Tag", assemblyPath, "FirstCommand.CreateTag");
                PushButtonData buttonData15 = new PushButtonData("Create Text", "Text", assemblyPath, "FirstCommand.CreateText");
                PushButtonData buttonData16 = new PushButtonData("Take Element To Combobox", "Combobox", assemblyPath, "FirstCommand.Combobox");
                PushButtonData buttonData17 = new PushButtonData("TwoWall", "Dim2Wall", assemblyPath, "FirstCommand.DimTwoElements");
                PushButtonData buttonData18 = new PushButtonData("View to combobox", "Combobox View", assemblyPath, "FirstCommand.ComboboxView");
                PushButtonData buttonData19 = new PushButtonData("Command Filter", "Command1", assemblyPath, "FirstCommand.Command1Filter");
                PushButtonData buttonData20 = new PushButtonData("Command Elevator", "Command2", assemblyPath, "FirstCommand.Command2Elevator");
                //PushButtonData buttonData21 = new PushButtonData("Command Segment", "Command3", assemblyPath, "FirstCommand.Command3Segment");

                buttonData1.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData2.LargeImage = new BitmapImage(new Uri(path + @"\wall.png"));
                buttonData3.Image = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData4.Image = new BitmapImage(new Uri(path + @"\wall.png"));
                buttonData5.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData6.LargeImage = new BitmapImage(new Uri(path + @"\wall.png"));
                buttonData7.LargeImage = new BitmapImage(new Uri(path + @"\wall.png"));
                buttonData8.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData9.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData10.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData11.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData12.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData13.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData14.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData15.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData16.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData17.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData18.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData19.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                buttonData20.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));
                //buttonData21.LargeImage = new BitmapImage(new Uri(path + @"\table.png"));

                //Add tooltip
                buttonData7.ToolTip = "Create a cloumn";
                buttonData7.ToolTipImage = new BitmapImage(new Uri(path + @"\wall.png"));

                //PushButton pushButton = ribbonPanel.AddItem(buttonData1) as PushButton;
                ribbonPanel.AddItem(buttonData1);
                ribbonPanel.AddItem(buttonData2);
                ribbonPanel.AddSeparator();
                ribbonPanel.AddStackedItems(buttonData3, buttonData4);
                ribbonPanel.AddSeparator();

                SplitButtonData sb1 = new SplitButtonData("splitBtn1", "split");
                SplitButton sb = ribbonPanel.AddItem(sb1) as SplitButton;

                sb.AddPushButton(buttonData5);
                sb.AddPushButton(buttonData6);
                ribbonPanel.AddSeparator();
                ribbonPanel.AddItem(buttonData7);
                ribbonPanel.AddItem(buttonData8);
                ribbonPanel.AddItem(buttonData9);
                ribbonPanel.AddItem(buttonData10);
                ribbonPanel.AddItem(buttonData11);
                ribbonPanel.AddItem(buttonData12);
                ribbonPanel.AddItem(buttonData13);
                ribbonPanel.AddItem(buttonData14);
                ribbonPanel.AddItem(buttonData15);
                ribbonPanel.AddItem(buttonData16);
                ribbonPanel.AddItem(buttonData17);
                ribbonPanel.AddItem(buttonData18);
                ribbonPanel.AddItem(buttonData19);
                ribbonPanel.AddItem(buttonData20);
                //ribbonPanel.AddItem(buttonData21);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }
        }
    }
}