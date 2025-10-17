using System;

namespace ASI.Basecode.Services.ServiceModels
{
    public class PendingUserRegistrationViewModel
    {
        public int Id { get; set; }
        public string EmailId { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string VerificationToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public bool IsVerified { get; set; }
        public int Role { get; set; }
        public int? OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public string OrganizationType { get; set; }
    }
}

