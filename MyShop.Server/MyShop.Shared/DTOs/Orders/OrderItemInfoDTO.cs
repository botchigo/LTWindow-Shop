using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Orders
{
    public record OrderItemInfoDTO
    {
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int Quantity { get; set; }

        [Required]
        public int ProductId { get; set; }

    }
}
