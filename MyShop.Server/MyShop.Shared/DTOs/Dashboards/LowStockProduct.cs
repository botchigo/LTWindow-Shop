namespace MyShop.Shared.DTOs.Dashboards
{
    public class LowStockProduct
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Stock { get; set; }
    }
}
