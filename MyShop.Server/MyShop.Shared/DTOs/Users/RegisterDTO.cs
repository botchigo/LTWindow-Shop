using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Users
{
    public record RegisterDTO
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
    }
}
