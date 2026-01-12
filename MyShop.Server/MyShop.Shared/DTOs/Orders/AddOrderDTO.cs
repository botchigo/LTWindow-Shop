using MyShop.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Orders
{
    public record AddOrderDTO
    {
        [Required]
        public OrderStatus Status { get; init; } = OrderStatus.Created;

        [Required]
        public PaymentMethod PaymentMethod { get; init; }

        [Required]
        public int UserId { get; init; }

        public IEnumerable<OrderItemInfoDTO> OrderItems { get; init; } = new List<OrderItemInfoDTO>();
    }
}
