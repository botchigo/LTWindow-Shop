using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Products
{
    public record AddProductDTO : ProductBaseDTO
    {
        [Required]
        public int CategoryId { get; init; }

        public IEnumerable<string> ImagePaths { get; init; } = new List<string>();
    }
}
