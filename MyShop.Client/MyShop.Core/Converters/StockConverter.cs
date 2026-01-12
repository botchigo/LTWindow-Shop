using Microsoft.UI.Xaml.Data;

namespace MyShop.Core.Converters
{
    public class StockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int stock)
            {
                return $"Còn {stock}";
            }
            return "Còn 0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
