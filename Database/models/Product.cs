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
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        [Column("import_price")]
        public decimal ImportPrice { get; set; }

        [Column("sale_price")]
        public decimal SalePrice { get; set; }

        public int Count { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        //Category
        [Column("category_id")]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }
    }
}
