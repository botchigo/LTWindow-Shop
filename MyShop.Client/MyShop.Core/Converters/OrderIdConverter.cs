using Microsoft.UI.Xaml.Data;

namespace MyShop.Core.Converters
{
    public class OrderIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int orderId)
            {
                return $"Đơn #{orderId}";
            }
            return "Đơn #0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
