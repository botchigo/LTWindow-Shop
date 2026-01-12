namespace MyShop.Core.DTOs
{
    public class ReportDataPointDto
    {
        public DateTimeOffset Date { get; set; }
        public double Value { get; set; }
        public double Profit { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}
