using Microsoft.UI.Xaml.Data;
using System.Globalization;

namespace MyShop.Core.Converters
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "N/A";

            if (value is DateTime date)
            {
                if (date == DateTime.MinValue) return "-";

                string? format = parameter as string;

                if (!string.IsNullOrEmpty(format) && format == "Full")
                {
                    return date.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
                }

                return date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            if (value is DateTimeOffset dateOffset)
            {
                return dateOffset.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
