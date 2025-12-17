using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.models
{
    [Table("product")]
    public class Product : AuditableEntity
    {
        [Key]
        [Column("product_id")]
        public int Id { get; set; }

        [Column("sku")]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("import_price")]
        public decimal ImportPrice { get; set; }

        [Required]
        [Column("sale_price")]
        public decimal SalePrice { get; set; }

        [Column("sale_amount")]
        public int SaleAmount { get; set; }

        [Column("stock")]
        public int Stock { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        //Category
        [Column("category_id")]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }

        //Images
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}
