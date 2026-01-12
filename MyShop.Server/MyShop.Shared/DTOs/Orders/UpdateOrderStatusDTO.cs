using MyShop.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Orders
{
    public record UpdateOrderStatusDTO
    {
        [Required]
        public int Id { get; init; }

        [Required]
        public OrderStatus NewStatus { get; init; }
    }
}
