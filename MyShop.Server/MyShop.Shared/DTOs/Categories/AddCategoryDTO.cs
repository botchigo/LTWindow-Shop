using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Shared.DTOs.Categories
{
    public record AddCategoryDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; init; } = string.Empty;

        [StringLength(200)]
        public string Description { get; init; } = string.Empty;
    }
}
