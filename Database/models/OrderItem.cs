using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.models
{
    [Table("order_item")]
    public class OrderItem
    {
        [Key]
        [Column("order_item_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemId { get; set; }

        [Column("order_id")]
        public int? OrderId { get; set; }

        [Column("product_id")]
        public int? ProductId { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column("unit_sale_price")]
        public int UnitSalePrice { get; set; }

        [Required]
        [Column("unit_cost")]
        public int UnitCost { get; set; }

        [Required]
        [Column("total_price")]
        public int TotalPrice { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
