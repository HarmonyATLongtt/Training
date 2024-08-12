using Autodesk.Revit.UI;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace SolutionRevitAPI
{
    public class MyApp : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Solution Revit API";
            application.CreateRibbonTab(tabName);

            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Main action");
            string path = Assembly.GetExecutingAssembly().Location;

            // PulldownButton
            PulldownButtonData ActionWithInstanceBtn = new PulldownButtonData("ActionWithInstance", " Action With Element");
            PulldownButton pulldownButton = panel.AddItem(ActionWithInstanceBtn) as PulldownButton;
            pulldownButton.AddPushButton(new PushButtonData("CreatNewInstance", "Creat New Instance", path, "SolutionRevitAPI.Commands.CreatNewInstance"));
            pulldownButton.AddPushButton(new PushButtonData("RotateElement", "Rotate Element", path, "SolutionRevitAPI.Commands.RotateElement"));
            pulldownButton.AddPushButton(new PushButtonData("DeleteElement", "Delete Element", path, "SolutionRevitAPI.Commands.DeleteElement"));
            pulldownButton.LargeImage = pulldownButton.Image = ConvertToBitmapSource(Properties.Resources.Creat, 32, 32);

            // PushButton
            PushButtonData GetParameterBtn = new PushButtonData("GetParameter", "Get Parameter", path, "SolutionRevitAPI.Commands.GetParameter");
            GetParameterBtn.LargeImage = GetParameterBtn.Image = ConvertToBitmapSource(Properties.Resources.find, 32, 32);
            GetParameterBtn.ToolTip = "Bạn có thể lấy dữ liệu thông tin Parameter của dối tượng";
            panel.AddItem(GetParameterBtn);

            PushButtonData EditParameterBtn = new PushButtonData("EditParameter", "Edit Parameter", path, "SolutionRevitAPI.Commands.EditParameter");
            EditParameterBtn.LargeImage = EditParameterBtn.Image = ConvertToBitmapSource(Properties.Resources.editpara, 32, 32);
            EditParameterBtn.ToolTip = "Bạn có thể thay đổi dữ liệu thông tin Parameter của dối tượng";
            panel.AddItem(EditParameterBtn);

            PushButtonData DistanceBetweenGridsBtn = new PushButtonData("DistanceBetweenGrids", "Distance 2 Grids", path, "SolutionRevitAPI.Commands.DistanceBetweenGrids");
            DistanceBetweenGridsBtn.LargeImage = DistanceBetweenGridsBtn.Image = ConvertToBitmapSource(Properties.Resources.distance, 32, 32);
            DistanceBetweenGridsBtn.ToolTip = "Tính toán khoảng cách giữa 2 grid song song, bạn có thể thay đổi khoảng cách của chúng";
            panel.AddItem(DistanceBetweenGridsBtn);

            PushButtonData EditFilterForViewBtn = new PushButtonData("EditFilterForView", "Edit & Creat Filter", path, "SolutionRevitAPI.Commands.CreatFilterForView");
            EditFilterForViewBtn.LargeImage = EditFilterForViewBtn.Image = ConvertToBitmapSource(Properties.Resources.filter, 32, 32);
            EditFilterForViewBtn.ToolTip = "Thêm xóa Filter cho các View";
            panel.AddItem(EditFilterForViewBtn);

            return Result.Succeeded;
        }

        public BitmapImage ConvertToBitmapSource(Image img, int width, int height)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                img.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.DecodePixelWidth = width;  // Đặt chiều rộng mong muốn
                bitmapImage.DecodePixelHeight = height; // Đặt chiều cao mong muốn
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}