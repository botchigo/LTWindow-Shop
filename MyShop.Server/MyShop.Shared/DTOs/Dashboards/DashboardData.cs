namespace MyShop.Shared.DTOs.Dashboards
{
    public class DashboardData
    {
        public int TotalProducts { get; set; }
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public List<LowStockProduct> LowStockProducts { get; set; } = new();
        public List<BestSellerProduct> BestSellerProducts { get; set; } = new();
        public List<RecentOrder> RecentOrders { get; set; } = new();
        public List<DailyRevenue> MonthlyRevenue { get; set; } = new();
    }
}
