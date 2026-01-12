using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace MyShop.Core.Converters
{
    public class ItemClickEventArgsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var args = value as ItemClickEventArgs;
            if (args == null)
                return null;

            return args.ClickedItem;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
