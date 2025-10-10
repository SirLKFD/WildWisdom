using System;

namespace ASI.Basecode.Data.Models
{
    public partial class RenewalPolicy
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public bool AllowRenewals { get; set; }
        public int MaxRenewals { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual Organization Organization { get; set; }
    }
}

