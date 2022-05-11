using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;

namespace WPF_Sample.Utils
{
    internal class Utils
    {
        /// <summary>
        /// Check file is using by another process
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileInUse(string filePath)
        {
            bool fileInUse = true;
            FileStream stream = null;
            try
            {
                stream = new FileInfo(filePath).Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                fileInUse = false;
            }
            catch
            {
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }
            return fileInUse;
        }

        /// <summary>
        /// Convert bitmap to image format
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        internal static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            //Credit for this method: https://stackoverflow.com/questions/22499407/how-to-display-a-bitmap-in-a-wpf-image
            using MemoryStream memory = new MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;
            BitmapImage bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();
            return bitmapimage;
        }
    }
}