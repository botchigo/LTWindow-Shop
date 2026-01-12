namespace MyShop.Shared.DTOs.Reports
{
    public record ProductSeries
    {
        public string ProductName { get; init; } = string.Empty;
        public List<ReportDataPoint> Points { get; init; } = new();
        public string ColorHex { get; init; } = string.Empty;
    }
}
