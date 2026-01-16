using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace MyShop.Core.Converters
{
    public class PathToImageConverter : IValueConverter
    {
        // Đường dẫn ảnh mặc định nếu dữ liệu null hoặc lỗi
        private const string PlaceholderPath = "ms-appx:///Assets/0_0_0.png";

        // Thư mục chứa ảnh sản phẩm
        private const string BaseProductFolder = "Assets/";

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string? path = null;

            // TRƯỜNG HỢP 1: Value là chuỗi (Binding Path)
            if (value is string s)
            {
                path = s;
            }
            // TRƯỜNG HỢP 2: Value là Object (Binding cả item), dùng Reflection lấy property "Path"
            else if (value != null)
            {
                var prop = value.GetType().GetProperty("Path");
                if (prop != null)
                {
                    path = prop.GetValue(value) as string;
                }
            }

            // Nếu không lấy được đường dẫn, trả về Placeholder
            if (string.IsNullOrEmpty(path))
            {
                return new BitmapImage(new Uri(PlaceholderPath));
            }

            try
            {
                Uri imageUri;

                // 1. Xử lý đường dẫn tuyệt đối (C:\Users\...) - Dùng khi chọn ảnh từ máy tính
                if (Path.IsPathRooted(path))
                {
                    imageUri = new Uri(path);
                }
                // 2. Xử lý đường dẫn mạng (http://...) - Dùng nếu sau này lưu ảnh online
                else if (Uri.TryCreate(path, UriKind.Absolute, out Uri? absoluteUri)
                         && (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
                {
                    imageUri = absoluteUri;
                }
                // 3. Xử lý đường dẫn tương đối (Assets/...)
                else
                {
                    // Chuẩn hóa dấu gạch chéo
                    string cleanPath = path.Replace("\\", "/");

                    // Nếu path chỉ là tên file ("abc.png") -> Thêm folder "Assets/Products/"
                    if (!cleanPath.Contains("/"))
                    {
                        cleanPath = $"{BaseProductFolder}{cleanPath}";
                    }

                    // Nếu path chưa có tiền tố ms-appx -> Thêm vào
                    if (!cleanPath.StartsWith("ms-appx:///"))
                    {
                        // Đảm bảo không bị dư dấu / ở đầu
                        cleanPath = cleanPath.TrimStart('/');
                        imageUri = new Uri($"ms-appx:///{cleanPath}");
                    }
                    else
                    {
                        imageUri = new Uri(cleanPath);
                    }
                }

                // Tắt cache để tiết kiệm bộ nhớ nếu load nhiều ảnh
                var bitmap = new BitmapImage(imageUri);
                // bitmap.DecodePixelWidth = 200; // Có thể bật cái này để tối ưu hiệu năng nếu ảnh quá to
                return bitmap;
            }
            catch
            {
                // Gặp bất cứ lỗi gì về format đường dẫn -> Trả về ảnh placeholder
                return new BitmapImage(new Uri(PlaceholderPath));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
