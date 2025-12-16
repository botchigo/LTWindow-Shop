using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;

            if (parameter is string formatString && value is IFormattable formattable)
            {
                return formattable.ToString(formatString, null);
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
