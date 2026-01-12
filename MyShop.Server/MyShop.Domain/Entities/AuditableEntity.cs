using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop.Domain.Entities
{
    public abstract class AuditableEntity
    {
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
