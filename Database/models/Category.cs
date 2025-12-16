using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.models
{
    [Table("category")]
    public class Category : AuditableEntity
    {
        [Key]
        [Column("category_id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        //Products
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
