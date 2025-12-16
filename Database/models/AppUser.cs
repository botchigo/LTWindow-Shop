using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.models
{
    [Table("app_user")]
    public class AppUser
    {
        [Key]
        [Column("user_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [Column("username")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column("password")]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Column("full_name")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }
    }
}
