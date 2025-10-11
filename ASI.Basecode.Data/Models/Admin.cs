using System;

namespace ASI.Basecode.Data.Models
{
    public partial class Admin
    {
        public int Id { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
        public int? OrganizationId { get; set; }
        public string ProfilePicture { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual Organization Organization { get; set; }
    }
}

