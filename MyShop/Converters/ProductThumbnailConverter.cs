using Database.models;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShop.Converters
{
    public class ProductThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var placeholder = new BitmapImage(new Uri("ms-appx:///Assets/placeholder.png"));

            if (value is IEnumerable<ProductImage> images)
            {
                var firstImage = images.FirstOrDefault();

                if (firstImage != null && !string.IsNullOrEmpty(firstImage.Path))
                {
                    string path = firstImage.Path;
                    try
                    {
                        Uri imageUri;
                        if (System.IO.Path.IsPathRooted(path))
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
                        return placeholder;
                    }
                }
            }

            return placeholder;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
