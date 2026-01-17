using Microsoft.UI.Xaml.Data;

namespace MyShop.Core.Converters
{
    public class DecimalToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is decimal d)
            {
                return (double)d;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is double d)
            {
                if (double.IsNaN(d)) return 0m;
                try
                {
                    return (decimal)d;
                }
                catch
                {
                    return 0m;
                }
            }
            return 0m;
        }
    }
}
