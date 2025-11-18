using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Role { get; set; } // 0 = User, 1 = Admin
        public int? OrganizationId { get; set; }
        public string ProfilePicture { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }
        public bool IsVerified { get; set; }
        public bool ReceiveOverdueEmails { get; set; }

        public virtual Organization Organization { get; set; }
    }
}
