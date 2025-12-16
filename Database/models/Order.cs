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

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("created_time")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        [Required]
        [Column("final_price")]
        public decimal FinalPrice { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

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
