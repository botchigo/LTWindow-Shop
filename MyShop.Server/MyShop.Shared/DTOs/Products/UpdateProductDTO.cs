using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Products
{
    public record UpdateProductDTO : ProductBaseDTO
    {
        [Required]
        public int Id { get; init; }        

        [Required]
        public int CategoryId { get; init; }

        public IEnumerable<string> NewImagePaths { get; init; } = new List<string>();
        public IEnumerable<int> ImageIdToDelete { get; init; } = new List<int>();
    }
}
