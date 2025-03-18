using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CrosshairOverlayApp
{
    public class FileToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string filePath && File.Exists(filePath))
            {
                try
                {
                    // Read the file into a byte array and then into a MemoryStream.
                    byte[] imageData = File.ReadAllBytes(filePath);
                    using (var memoryStream = new MemoryStream(imageData))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memoryStream;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}