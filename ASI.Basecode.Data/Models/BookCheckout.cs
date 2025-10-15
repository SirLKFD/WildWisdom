using System;

namespace ASI.Basecode.Data.Models
{
    public partial class BookCheckout
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OrgBookId { get; set; }
        public int OrganizationId { get; set; }
        public DateTime AddedAt { get; set; }
        
        // Audit fields
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual OrgBooks OrgBook { get; set; }
        public virtual Organization Organization { get; set; }
    }
}
