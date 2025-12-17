using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.models
{
    [Table("product_image")]
    public class ProductImage 
    {
        [Key]
        [Column("image_id")]
        public int Id { get; set; }

        [Required]
        [Column("image_path")]
        public string Path { get; set; } = string.Empty;

        //Product
        [Column("product_id")]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
    }
}
