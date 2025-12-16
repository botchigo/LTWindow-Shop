using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.models
{
    [Table("order_item")]
    public class OrderItem
    {
        [Key]
        [Column("order_item_id")]
        public int Id { get; set; }
        public int Quantity { get; set; }

        [Column("unit_sale_price")]
        public decimal UnitSalePrice { get; set; }

        [Column("unit_cost")]
        public decimal UnitCost { get; set; }

        [Column("total_price")]
        public decimal TotalPrice { get; set; }

        //Order
        [Column("order_id")]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        //Product
        [Column("product_id")]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
    }
}
