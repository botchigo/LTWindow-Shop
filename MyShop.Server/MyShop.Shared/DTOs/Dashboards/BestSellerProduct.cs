namespace MyShop.Shared.DTOs.Dashboards
{
    public class BestSellerProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public long TotalQuantity { get; set; }
    }
}
