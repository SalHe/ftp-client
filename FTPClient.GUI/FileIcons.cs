using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FTPClient.GUI
{
    public class FileIcons
    {
        private static Dictionary<string, BitmapImage> _fileIcons = new();

        public static BitmapImage GetFileIcon(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            BitmapImage bitmapImage;
            try
            {
                bitmapImage = _fileIcons[extension];
            }
            catch (KeyNotFoundException)
            {
                string tempFilePath = Path.Join(Path.GetTempPath(), Guid.NewGuid() + "." + extension);
                File.Create(tempFilePath).Close();

                using MemoryStream memory = new MemoryStream();
                var bitmap = Icon.ExtractAssociatedIcon(tempFilePath)?.ToBitmap();
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                _fileIcons.Add(extension, bitmapImage);

                File.Delete(tempFilePath);
            }
            return bitmapImage;
        }
    }
}
