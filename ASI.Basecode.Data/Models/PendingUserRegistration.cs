using System;

namespace ASI.Basecode.Data.Models
{
    public partial class PendingUserRegistration
    {
        public int Id { get; set; }
        public string EmailId { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string EncryptedOTP { get; set; }
        public string VerificationToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public bool IsVerified { get; set; }
        public int Role { get; set; } // 0 = User, 1 = Admin
        public int? OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationType { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}

