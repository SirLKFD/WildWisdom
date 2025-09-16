using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace ASI.Basecode.Data.Models
{
    public partial class LogMenu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogMenuID { get; set; }

        [ForeignKey(nameof(Menu))]
        public int MenuID { get; set; }
        public Menu Menu { get; set; }

        [StringLength(100)]
        public string ActionType { get; set; }

        [StringLength(100)]
        public string FieldChange { get; set; }

        [StringLength(255)]
        public string OldValue { get; set; }

        [StringLength(255)]
        public string NewValue { get; set; }

        [ForeignKey(nameof(User))]
        public int UpdatedBy { get; set; }
        public User User { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime DateCreated { get; set; }
    }
}
