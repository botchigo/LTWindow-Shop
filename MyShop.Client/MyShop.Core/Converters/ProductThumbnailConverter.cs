using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections;

namespace MyShop.Core.Converters
{
    public class ProductThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                var collection = value as IEnumerable;
                if (collection == null) return GetPlaceholder();

                // 2. Lấy phần tử đầu tiên trong danh sách
                var firstImageObj = collection.Cast<object>().FirstOrDefault();
                if (firstImageObj == null) return GetPlaceholder();

                // 3. Dùng Reflection để lấy giá trị property "Path"
                var type = firstImageObj.GetType();
                var pathProperty = type.GetProperty("Path");

                if (pathProperty == null) return GetPlaceholder();

                var pathValue = pathProperty.GetValue(firstImageObj) as string;

                if (string.IsNullOrEmpty(pathValue)) return GetPlaceholder();

                if (pathValue.Contains(":") || pathValue.StartsWith("\\"))
                {
                    return new BitmapImage(new Uri(pathValue));
                }

                // 4. Tạo đường dẫn tuyệt đối tới Assets
                return new BitmapImage(new Uri($"ms-appx:///Assets/{pathValue}"));
            }
            catch
            {
                return GetPlaceholder();
            }
        }

        private BitmapImage GetPlaceholder()
        {
            // Đường dẫn ảnh mặc định nếu sản phẩm không có ảnh hoặc lỗi
            return new BitmapImage(new Uri("ms-appx:///Assets/0_0_0.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
