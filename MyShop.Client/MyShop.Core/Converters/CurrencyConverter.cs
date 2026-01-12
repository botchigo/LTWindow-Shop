using Microsoft.UI.Xaml.Data;
using System.Globalization;

namespace MyShop.Core.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "0 đ";

            if (decimal.TryParse(value.ToString(), out decimal money))
            {
                var culture = new CultureInfo("vi-VN");

                return money.ToString("N0", culture) + " đ";
            }

            return "0 đ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
