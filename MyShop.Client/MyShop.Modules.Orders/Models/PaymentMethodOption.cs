using MyShop.Infrastructure;

namespace MyShop.Modules.Orders.Models
{
    public class PaymentMethodOption
    {
        public string DisplayName { get; set; } 
        public PaymentMethod Value { get; set; }
    }
}
