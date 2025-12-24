namespace MyShop.Shared.DTOs.Reports
{
    public class ReportDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; } 
        public double Profit { get; set; } 
        public DateTime Date { get; set; }
    }
}
