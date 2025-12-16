using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.models
{
    [Table("product")]
    public class Product
    {
        [Key]
        [Column("product_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Column("category_id")]
        public int? CategoryId { get; set; }

        [Required]
        [Column("sku")]
        [StringLength(50)]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [Column("name")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("import_price")]
        public int ImportPrice { get; set; }

        [Required]
        [Column("sale_price")]
        public int SalePrice { get; set; }

        [Column("count")]
        public int Count { get; set; } = 0;

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
