using Database.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.models
{
    [Table("order")]
    public class Order : AuditableEntity
    {
        [Key]
        [Column("order_id")]
        public int Id { get; set; }        

        [Required]
        [Column("final_price")]
        public decimal FinalPrice { get; set; }

        [Column("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Created;

        [Column("payment_method")]
        public PaymentMethod PaymentMethod { get; set; }

        //User
        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; }

        //OrderItems
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
