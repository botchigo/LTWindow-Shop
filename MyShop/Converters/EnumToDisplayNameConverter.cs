using Database.Enums;
using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Converters
{
    public class EnumToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PaymentMethod method)
            {
                return method switch
                {
                    PaymentMethod.COD => "Thanh toán khi nhận hàng (COD)",
                    PaymentMethod.MOMO => "Ví điện tử Momo",
                    PaymentMethod.VNPAY => "Ví điện tử VNPay",
                    _ => method.ToString()
                };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
