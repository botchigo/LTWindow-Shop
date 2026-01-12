using Microsoft.UI.Xaml.Data;

namespace MyShop.Core.Converters
{
    public class DecimalToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal decimalValue)
            {
                return System.Convert.ToDouble(decimalValue);
            }
            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double doubleValue)
            {
                if (double.IsNaN(doubleValue)) return null;
                return System.Convert.ToDecimal(doubleValue);
            }
            return null;
        }
    }
}
