using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections;

namespace MyShop.Core.Converters
{
    public class ProductThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var placeholder = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));

            if (value == null) return placeholder;

            string? imagePath = null;

            if (value is string pathStr)
            {
                imagePath = pathStr;
            }
            else if (value is IEnumerable list)
            {
                var enumerator = list.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    var firstItem = enumerator.Current;
                    if (firstItem is string firstPath)
                    {
                        imagePath = firstPath;
                    }
                    else
                    {
                        imagePath = GetPathProperty(firstItem);
                    }
                }
            }
            else
            {
                imagePath = GetPathProperty(value);
            }

            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    Uri imageUri;

                    if (Path.IsPathRooted(imagePath) || imagePath.StartsWith("http"))
                    {
                        imageUri = new Uri(imagePath);
                    }
                    else
                    {
                        string relativePath = imagePath.Replace("\\", "/");
                        if (!relativePath.StartsWith("ms-appx:///") && !relativePath.StartsWith("ms-appdata://"))
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
                    return placeholder;
                }
            }

            return placeholder;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private string? GetPathProperty(object? obj)
        {
            if (obj == null) return null;

            if (obj is string s) return s;

            try
            {
                dynamic item = obj;
                return item.Path; 
            }
            catch
            {
                var prop = obj.GetType().GetProperty("Path");
                return prop?.GetValue(obj)?.ToString();
            }
        }
    }
}
