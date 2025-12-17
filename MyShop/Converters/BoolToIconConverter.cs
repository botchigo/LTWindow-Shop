using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Converters
{
    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isEditMode && isEditMode)
            {
                return "\uE70F";
            }

            return "\uE710";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
