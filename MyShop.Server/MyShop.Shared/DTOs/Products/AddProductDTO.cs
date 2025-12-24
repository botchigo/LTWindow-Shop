using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Products
{
    public record AddProductDTO
    {
        [Required]
        public string Name { get; init; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập không được âm")]
        public decimal ImportPrice { get; init; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá bán không được âm")]
        public decimal SalePrice { get; init; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int Stock { get; init; }

        [Required]
        [StringLength(1000)]
        public string Description { get; init; } = string.Empty;

        [Required]
        public int CategoryId { get; init; }
        public IEnumerable<string> ImagePaths { get; init; } = new List<string>();
    }
}
