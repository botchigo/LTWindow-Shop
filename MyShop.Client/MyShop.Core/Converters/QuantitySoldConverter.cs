using Microsoft.UI.Xaml.Data;

namespace MyShop.Core.Converters
{
    public class QuantitySoldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int quantity)
            {
                return $"Đã bán: {quantity}";
            }
            return "Đã bán: 0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
