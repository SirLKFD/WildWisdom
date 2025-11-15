using System;

namespace ASI.Basecode.Data.Models
{
    public partial class FinePolicy
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public decimal FinePerDay { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual Organization Organization { get; set; }
    }
}

