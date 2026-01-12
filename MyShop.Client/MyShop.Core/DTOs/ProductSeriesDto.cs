namespace MyShop.Core.DTOs
{
    public class ProductSeriesDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = string.Empty;
        public List<ReportDataPointDto> Points { get; set; } = new();
    }
}
