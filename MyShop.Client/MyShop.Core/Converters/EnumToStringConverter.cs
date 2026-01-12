using Microsoft.UI.Xaml.Data;
using MyShop.Core.Enums;

namespace MyShop.Core.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SortCriteria criteria)
            {
                return criteria switch
                {
                    SortCriteria.Name => "Tên sản phẩm",
                    SortCriteria.Price => "Giá bán", // Hoặc "Giá nhập" tùy ngữ cảnh
                    SortCriteria.CreateDate => "Ngày tạo",
                    SortCriteria.UpdateDate => "Ngày cập nhật",
                    _ => value.ToString() ?? string.Empty
                };
            }

            if (value is SortDirection direction)
            {
                return direction switch
                {
                    SortDirection.Ascending => "Tăng dần",
                    SortDirection.Descending => "Giảm dần",
                    _ => value.ToString() ?? string.Empty
                };
            }

            if(value is ReportType reportType)
            {
                return reportType switch
                {
                    ReportType.ProductSales => "So sánh sản phẩm",
                    ReportType.RevenueProfit => "Doanh thu & Lợi nhuận",
                    _ => value?.ToString() ?? string.Empty
                };
            }

            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
