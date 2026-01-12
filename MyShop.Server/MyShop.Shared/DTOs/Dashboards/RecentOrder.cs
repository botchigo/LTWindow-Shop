using MyShop.Shared.Enums;

namespace MyShop.Shared.DTOs.Dashboards
{
    public class RecentOrder
    {
        public int OrderId { get; set; }
        public DateTime CreatedTime { get; set; }
        public decimal FinalPrice { get; set; }
        public OrderStatus Status { get; set; } 
        public PaymentMethod PaymentMethod { get; set; } 
    }
}
