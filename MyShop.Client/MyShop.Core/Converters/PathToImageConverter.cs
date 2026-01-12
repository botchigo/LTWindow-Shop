using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MyShop.Core.Converters
{
    public class PathToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string? path = value as string;

            if (string.IsNullOrEmpty(path))
            {
                // Trả về ảnh mặc định nếu null (Placeholder)
                return new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
            }

            try
            {
                Uri imageUri;

                if (Path.IsPathRooted(path))
                {
                    imageUri = new Uri(path);
                }
                else
                {
                    string relativePath = path.Replace("\\", "/");
                    if (!relativePath.StartsWith("ms-appx:///"))
                    {
                        imageUri = new Uri("ms-appx:///" + relativePath);
                    }
                    else
                    {
                        imageUri = new Uri(relativePath);
                    }
                }

                return new BitmapImage(imageUri);
            }
            catch
            {
                return new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
