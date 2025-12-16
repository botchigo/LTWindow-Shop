using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.models
{
    public abstract class AuditableEntity
    {
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
