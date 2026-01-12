using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MyShop.Core.Enums;

namespace MyShop.Core.Converters
{
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var defaultColor = new SolidColorBrush(Colors.Gray);

            if (value == null) return defaultColor;

            string? status = value.ToString();

            if (string.Equals(status, nameof(OrderStatus.Created), StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Colors.Orange);
            }

            if (string.Equals(status, nameof(OrderStatus.Paid), StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Colors.SeaGreen);
            }

            if (string.Equals(status, nameof(OrderStatus.Canceled), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Colors.IndianRed);
            }

            return defaultColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
