using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("created_time")]
        public DateTime CreatedTime { get; set; } = DateTime.Now;

        [Required]
        [Column("final_price")]
        public int FinalPrice { get; set; } = 0;

        [Column("status")]
        [StringLength(30)]
        public string Status { get; set; } = "paid";

        [Column("payment_method")]
        [StringLength(30)]
        public string PaymentMethod { get; set; } = "cash";

        [ForeignKey("UserId")]
        public virtual AppUser? User { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
