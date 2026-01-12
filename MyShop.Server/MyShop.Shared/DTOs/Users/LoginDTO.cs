using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Shared.DTOs.Users
{
    public record LoginDTO
    {
        [Required]
        [StringLength(50)]
        public string Username { get; init; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; init; } = string.Empty;
    }
}
