using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IPendingUserRegistrationRepository
    {
        void AddPending(PendingUserRegistration pending);
        PendingUserRegistration GetByEmail(string emailId);
        PendingUserRegistration GetByToken(string token);
        void UpdateVerificationStatus(int id, bool isVerified);
        void DeletePending(int id);
        void CleanupExpiredTokens();
        bool EmailExists(string emailId);
    }
}

