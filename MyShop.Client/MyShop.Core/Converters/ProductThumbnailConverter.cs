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
                // 1. Kiểm tra null hoặc danh sách rỗng
                var collection = value as IEnumerable;
                if (collection == null) return GetPlaceholder();

                // 2. Lấy phần tử đầu tiên trong danh sách
                // (Vì là Interface nên ta cast sang object để thao tác)
                var firstImageObj = collection.Cast<object>().FirstOrDefault();
                if (firstImageObj == null) return GetPlaceholder();

                // 3. Dùng Reflection để lấy giá trị property "Path"
                // Lý do: Class được sinh ra bởi Strawberry Shake có tên rất dài và có thể thay đổi,
                // Reflection giúp ta lấy data mà không cần quan tâm tên class cụ thể.
                var type = firstImageObj.GetType();
                var pathProperty = type.GetProperty("Path");

                if (pathProperty == null) return GetPlaceholder();

                var pathValue = pathProperty.GetValue(firstImageObj) as string;

                if (string.IsNullOrEmpty(pathValue)) return GetPlaceholder();

                // 4. Tạo đường dẫn tuyệt đối tới Assets
                // Lưu ý: Đảm bảo ảnh nằm trong Assets/Products như bạn đã cấu hình
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
