using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASI.Basecode.Data.Models
{
    public partial class Menu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MenuID { get; set; }

        [Required]
        [StringLength(100)]
        public string MenuName { get; set; }

        [Required]
        [StringLength(100)]
        public string MenuDescription { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        [Column("IsDeleted", TypeName = "tinyint")]
        public bool IsDeleted { get; set; }

        public virtual ICollection<LogMenu> LogMenus { get; set; }
    }
}
