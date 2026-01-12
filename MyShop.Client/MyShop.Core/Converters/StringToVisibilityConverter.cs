using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Core.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string? text = value as string;

            bool hasText = !string.IsNullOrWhiteSpace(text);

            return hasText ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
