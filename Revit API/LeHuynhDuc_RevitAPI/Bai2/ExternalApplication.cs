using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Bai2
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.ReadOnly)]
    internal class ExternalApplication : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            //create ribbon
            String tabName = "___Demo___";
            application.CreateRibbonTab(tabName);
            string path = Assembly.GetExecutingAssembly().Location;
            //BUTTON//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            PushButtonData btn1 = new PushButtonData("Select", "SelectTxt", path, "Bai2.MyFirstCommand");
            PushButtonData btn2 = new PushButtonData("Hello", "HelloTxt", path, "Bai2.HelloWorld");
            PushButtonData btn3 = new PushButtonData("SelectID", "SelectID", path, "Bai2.Selection_Element");
            PushButtonData btn4 = new PushButtonData("FilterWall", "Filter_Wall", path, "Bai2.Filtered_wall");
            PushButtonData btn5 = new PushButtonData("FilterWindow", "Filter_Window", path, "Bai2.Filtered_window");
            PushButtonData btn6 = new PushButtonData("CreateWall", "Create_Wall", path, "Bai2.Create_Wall");
            PushButtonData btn7 = new PushButtonData("CreateFloor", "Create_Floor", path, "Bai2.Create_Floor");
            PushButtonData btn8 = new PushButtonData("FilterDoor", "Filter_Door", path, "Bai2.Filter_Door");
            PushButtonData btn9 = new PushButtonData("SlectGeometry", "Select_Geometry", path, "Bai2.Geometry_Solid");
            PushButtonData btn10 = new PushButtonData("LinePoin", "Line_Poin", path, "Bai2.Line_Left_Right");
            PushButtonData btn11 = new PushButtonData("FindIntersector", "Find_Intersector", path, "Bai2.Find_Intersector");
            PushButtonData btn12 = new PushButtonData("CreateFamily", "Create_Instance", path, "Bai2.Create_Instance");
            PushButtonData btn13 = new PushButtonData("CreateSchedule", "Create_Schedule", path, "Bai2.Create_Schedule");
            PushButtonData btn14 = new PushButtonData("CreateDimension", "Create_Dimension", path, "Bai2.Create_Dimension");
            PushButtonData btn15 = new PushButtonData("CreateSheet", "Create_Sheet", path, "Bai2.Create_Sheet");
            PushButtonData btn16 = new PushButtonData("AddViewSheet", "Add_View_Sheet", path, "Bai2.Add_View_Sheet");
            PushButtonData btn17 = new PushButtonData("CreateViewPlane", "Create_ViewPlane", path, "Bai2.Create_ViewPlane");
            PushButtonData btn18 = new PushButtonData("CreateTag", "Create_Tag", path, "Bai2.Create_Tag");
            PushButtonData btn19 = new PushButtonData("AddFieldSchedule", "Add_Field_Schedule", path, "Bai2.Add_Field_Schedule");
            PushButtonData btn20 = new PushButtonData("SortingGroupingSchedule", "Sorting_Grouping_Schedule", path, "Bai2.Sorting_Grouping_Schedule");
            PushButtonData btn21 = new PushButtonData("FilterSchedule", "Filter_Schedule", path, "Bai2.Filter_Schedule");
            PushButtonData btn22 = new PushButtonData("GroupingHeaderSchedule", "Grouping_Header_Schedule", path, "Bai2.Grouping_Header_Schedule");
            PushButtonData btn23 = new PushButtonData("FormatingSchedule", "Formating_Schedule", path, "Bai2.Formating_Schedule");
            PushButtonData btn24 = new PushButtonData("CreateView", "Create_View", path, "Bai2.Create_View");
            PushButtonData btn25 = new PushButtonData("CreateText", "Create_Text", path, "Bai2.Create_Text");
            PushButtonData btn26 = new PushButtonData("CreateGrid", "Create_Grid", path, "Bai2.Create_Grid");
            PushButtonData btn27 = new PushButtonData("CreateRegion", "Create_Region", path, "Bai2.Create_Region");
            PushButtonData btn28 = new PushButtonData("CreateLevel", "Create_Level", path, "Bai2.Create_Level");
            PushButtonData btn29 = new PushButtonData("RetrievingAllLevels", "Retrieving_All_Levels", path, "Bai2.Retrieving_All_Levels");
            //BUTTONICON////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            btn1.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\hello12.png"));
            btn2.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\select12.png"));
            btn3.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\select24.png"));
            btn4.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\wall24.png"));
            btn5.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\window24.png"));
            btn6.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\wall24_1.png"));
            btn7.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\floor24.png")); 
            btn8.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\door24.png"));
            btn10.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\line24.png"));
            btn11.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\Intersection24.png"));
            //PANEL////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            RibbonPanel panel1 = application.CreateRibbonPanel(tabName, "Family");
            RibbonPanel panel2 = application.CreateRibbonPanel(tabName, "Select");  
            RibbonPanel panel3 = application.CreateRibbonPanel(tabName, "Filter");
            RibbonPanel panel4 = application.CreateRibbonPanel(tabName, "Create");
            RibbonPanel panel5 = application.CreateRibbonPanel(tabName, "Other");
            RibbonPanel panel6 = application.CreateRibbonPanel(tabName, "Schedule");
            RibbonPanel panel7 = application.CreateRibbonPanel(tabName, "Sheet");
            RibbonPanel panel8 = application.CreateRibbonPanel(tabName, "View");
            RibbonPanel panel9 = application.CreateRibbonPanel(tabName, "Annotation");
            RibbonPanel panel10 = application.CreateRibbonPanel(tabName, "Level");
            //Radiobutton_Family////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // add radio button group
            RadioButtonGroupData radioData = new RadioButtonGroupData("radioGroup");
            RadioButtonGroup radioButtonGroup = panel1.AddItem(radioData) as RadioButtonGroup;
            // create toggle buttons and add to radio button group
            ToggleButtonData tb1 = new ToggleButtonData("toggleButton1", "Create_Family", path, "Bai2.Create_NewFamily");
            tb1.ToolTip = "Red Option";
            tb1.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\family24.png"));
            ToggleButtonData tb2 = new ToggleButtonData("toggleButton2", "Load_Family", path, "Bai2.Load_Family");
            tb2.ToolTip = "Green Option";
            tb2.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\family24.png"));
            ToggleButtonData tb3 = new ToggleButtonData("toggleButton3", "GetLoaded_Symbols", path, "Bai2.GetLoaded_Symbols");
            tb3.ToolTip = "Blue Option";
            tb3.LargeImage = new BitmapImage(new Uri(@"D:\ThucTap\Revit\Bai2\Bai2\Images\radio24.png"));
            radioButtonGroup.AddItem(tb1);
            radioButtonGroup.AddItem(tb2);
            radioButtonGroup.AddItem(tb3);
            //Select/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            SplitButtonData sbd1 = new SplitButtonData("Select", "select");
            SplitButton sb1 = panel2.AddItem(sbd1) as SplitButton;
            sb1.AddPushButton(btn1);
            sb1.AddPushButton(btn3);
            sb1.AddPushButton(btn9);
            //Filter/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            SplitButtonData sbd2 = new SplitButtonData("Filter", "filter");
            SplitButton sb2 = panel3.AddItem(sbd2) as SplitButton;
            sb2.AddPushButton(btn4);
            sb2.AddPushButton(btn5);
            sb2.AddPushButton(btn8);
            //Create////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            SplitButtonData sbd3 = new SplitButtonData("Create", "create");
            SplitButton sb3 = panel4.AddItem(sbd3) as SplitButton;
            sb3.AddPushButton(btn6);
            sb3.AddPushButton(btn7);
            sb3.AddPushButton(btn12);
            //Other//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            SplitButtonData sbd4 = new SplitButtonData("Other", "other");
            SplitButton sb4 = panel5.AddItem(sbd4) as SplitButton;
            sb4.AddPushButton(btn2);
            sb4.AddPushButton(btn10);
            sb4.AddPushButton(btn11);
            //Schedule//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            SplitButtonData sbd5 = new SplitButtonData("Schedule", "schedule");
            SplitButton sb5 = panel6.AddItem(sbd5) as SplitButton;
            sb5.AddPushButton(btn13);
            sb5.AddPushButton(btn19);
            sb5.AddPushButton(btn20);
            sb5.AddPushButton(btn21);
            sb5.AddPushButton(btn22);
            sb5.AddPushButton(btn23);
            //Sheet///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            SplitButtonData sbd6 = new SplitButtonData("Sheet", "sheet");
            SplitButton sb6 = panel7.AddItem(sbd6) as SplitButton;
            sb6.AddPushButton(btn15);
            sb6.AddPushButton(btn16);
            //View///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            SplitButtonData sbd7 = new SplitButtonData("View", "view");
            SplitButton sb7 = panel8.AddItem(sbd7) as SplitButton;
            sb7.AddPushButton(btn17);
            sb7.AddPushButton(btn24);
            //Annotation/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
            SplitButtonData sbd8 = new SplitButtonData("Annotation", "Annotation");
            SplitButton sb8 = panel9.AddItem(sbd8) as SplitButton;
            sb8.AddPushButton(btn14);
            sb8.AddPushButton(btn18);
            sb8.AddPushButton(btn25);
            sb8.AddPushButton(btn26);
            sb8.AddPushButton(btn27);
            //Level///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            SplitButtonData sbd9 = new SplitButtonData("Level", "Level");
            SplitButton sb9 = panel10.AddItem(sbd9) as SplitButton;
            sb9.AddPushButton(btn28);
            sb9.AddPushButton(btn29);
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            return Result.Succeeded;
        }

        public void ElementChangedEvent(object sender, DocumentChangedEventArgs args)
        {
           
        }
    }

}
