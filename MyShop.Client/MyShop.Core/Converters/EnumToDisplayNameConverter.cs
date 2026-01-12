using Microsoft.UI.Xaml.Data;

namespace MyShop.Core.Converters
{
    public class EnumToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return "";

            string enumString = value.ToString()?.ToUpper() ?? "";

            return enumString switch
            {
                "COD" => "Thanh toán khi nhận hàng (COD)",
                "MOMO" => "Ví điện tử Momo",
                "VNPAY" => "Ví điện tử VNPay",
                _ => value.ToString() ?? string.Empty 
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
