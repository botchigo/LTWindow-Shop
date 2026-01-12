using Microsoft.UI.Xaml.Data;

namespace MyShop.Core.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return string.Empty;

            if (parameter is string formatString && value is IFormattable formattable)
            {
                return formattable.ToString(formatString, null);
            }

            return value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
