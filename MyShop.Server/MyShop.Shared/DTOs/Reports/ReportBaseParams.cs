using MyShop.Shared.Enums;

namespace MyShop.Shared.DTOs.Reports
{
    public record ReportBaseParams(
        DateTime start,
        DateTime end,
        ReportTimeInterval interval);
}
