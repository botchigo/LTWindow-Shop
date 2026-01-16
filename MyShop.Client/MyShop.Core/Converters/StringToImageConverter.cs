using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MyShop.Core.Converters
{
    public class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var path = value as string;
                if (string.IsNullOrEmpty(path)) return GetPlaceholder();

                // TRƯỜNG HỢP 1: Ảnh vừa chọn từ máy tính (Đường dẫn tuyệt đối)
                if (path.Contains(":") || path.StartsWith("\\"))
                {
                    return new BitmapImage(new Uri(path));
                }

                // TRƯỜNG HỢP 2: Ảnh từ Server/Assets (Đường dẫn tương đối)       
                return new BitmapImage(new Uri($"ms-appx:///Assets/{path}"));
            }
            catch
            {
                return GetPlaceholder();
            }
        }

        private BitmapImage GetPlaceholder()
        {
            return new BitmapImage(new Uri("ms-appx:///Assets/0_0_0.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
