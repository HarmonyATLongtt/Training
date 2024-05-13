using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Sample
{
    internal class App : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            String tabName = "Tab New";
            application.CreateRibbonTab(tabName);
            // Create two push buttons
            PushButtonData button1 = new PushButtonData("Create Column",
            "Create Column", @"D:\Revit Learn\Sample\Sample\bin\Debug\Sample.dll", "Sample.CreateColumn");
            PushButtonData button2 = new PushButtonData("Create Wall",
            "Create Wall", @"D:\Revit Learn\Sample\Sample\bin\Debug\Sample.dll", "Sample.CreateWall");
            PushButtonData button3 = new PushButtonData("Create Floor",
             "Create Floor", @"D:\Revit Learn\Sample\Sample\bin\Debug\Sample.dll", "Sample.CreateFloor");
            PushButtonData button4 = new PushButtonData("Create Beam",
            "Create Beam", @"D:\Revit Learn\Sample\Sample\bin\Debug\Sample.dll", "Sample.CreateBeam");
            PushButtonData button5 = new PushButtonData("Buton5",
            "Button5", @"D:\Revit Learn\Sample\Sample\bin\Debug\Sample.dll", "Sample.Class1");
            RibbonPanel m_projectPanel = application.CreateRibbonPanel(tabName, "This Panel Name");

            m_projectPanel.AddItem(button1);
            m_projectPanel.AddItem(button2);
            m_projectPanel.AddItem(button3);
            m_projectPanel.AddStackedItems(button4, button5);
            SplitButtonData sb1 = new SplitButtonData("Button1", "Split");
            SplitButton sb = m_projectPanel.AddItem(sb1) as SplitButton;
            sb.AddPushButton(new PushButtonData("Buton6",
            "Button6", @"D:\Revit Learn\Sample\Sample\bin\Debug\Sample.dll", "Sample.Class1"));
            sb.AddPushButton(new PushButtonData("Buton7",
            "Button7", @"D:\Revit Learn\Sample\Sample\bin\Debug\Sample.dll", "Sample.Class1"));
            // Set the large image shown on button\
            m_projectPanel.AddSlideOut();
            m_projectPanel.AddItem(new PushButtonData("Buton8",
            "Button8", @"D:\Revit Learn\Sample\Sample\bin\Debug\Sample.dll", "Sample.Class1"));
            List<RibbonItem> projectButon = new List<RibbonItem>();
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("NewRibbonPanel");

            PushButton pushButton = ribbonPanel.AddItem(new PushButtonData("HelloWorld",
            "Sample", @"D:\Revit Learn\Sample\Sample\bin\Debug\Sample.dll", "Sample.Class1")) as PushButton;

            Uri uriImage = new Uri(@"D:\helloword.png");
            BitmapImage largeImage = new BitmapImage(uriImage);
            pushButton.LargeImage = largeImage;
            return Result.Succeeded;
            //application.DialogBoxShowing += new EventHandler<Autodesk.Revit.UI.Events.DialogBoxShowingEventArgs>(AppDialogShowing);
            //return Result.Succeeded;
        }

        private static void AddSlideOut(RibbonPanel panel)
        {
            string assembly = @"D:\Sample\HelloWorld\bin\Debug\Hello.dll";

            panel.AddSlideOut();

            // create some controls for the slide out
            PushButtonData b1 = new PushButtonData("ButtonName1", "Button 1",
                    assembly, "Hello.HelloButton");
            b1.LargeImage =
                    new System.Windows.Media.Imaging.BitmapImage(new Uri(@"D:\Sample\HelloWorld\bin\Debug\39-Globe_32x32.png"));
            PushButtonData b2 = new PushButtonData("ButtonName2", "Button 2",
                    assembly, "Hello.HelloTwo");
            b2.LargeImage =
                    new System.Windows.Media.Imaging.BitmapImage(new Uri(@"D:\Sample\HelloWorld\bin\Debug\39-Globe_16x16.png"));

            // items added after call to AddSlideOut() are added to slide-out automatically
            panel.AddItem(b1);
            panel.AddItem(b2);
        }
    }
}