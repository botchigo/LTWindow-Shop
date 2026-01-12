using MyShop.Shared.Enums;

namespace MyShop.Shared.DTOs.Reports
{
    public record GetSingleProductSeriesDTO(
        string productName, 
        DateTime start, 
        DateTime end, 
        ReportTimeInterval interval, 
        int colorIndex);
}
