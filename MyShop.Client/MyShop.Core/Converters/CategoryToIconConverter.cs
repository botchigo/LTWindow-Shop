using Microsoft.UI.Xaml.Data;

namespace MyShop.Core.Converters
{
    public class CategoryToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string categoryName && !string.IsNullOrEmpty(categoryName))
            {
                return categoryName.ToLower() switch
                {
                    var name when name.Contains("laptop") || name.Contains("máy tính") => "\uE7F8",
                    var name when name.Contains("điện thoại") || name.Contains("phone") => "\uE8EA",
                    var name when name.Contains("đồng hồ") || name.Contains("watch") => "\uE95F",
                    var name when name.Contains("tai nghe") || name.Contains("âm thanh") => "\uE7F5",
                    var name when name.Contains("phụ kiện") => "\uE7F4",
                    _ => "\uE7F4"
                };
            }
            return "\uE7F4";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
