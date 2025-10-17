using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using System;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class PendingUserRegistrationRepository : IPendingUserRegistrationRepository
    {
        private readonly AsiBasecodeDBContext _context;

        public PendingUserRegistrationRepository(AsiBasecodeDBContext context)
        {
            _context = context;
        }

        public void AddPending(PendingUserRegistration pending)
        {
            _context.PendingUserRegistrations.Add(pending);
            _context.SaveChanges();
        }

        public PendingUserRegistration GetByEmail(string emailId)
        {
            return _context.PendingUserRegistrations
                .FirstOrDefault(p => p.EmailId == emailId);
        }

        public PendingUserRegistration GetByToken(string token)
        {
            return _context.PendingUserRegistrations
                .FirstOrDefault(p => p.VerificationToken == token);
        }

        public void UpdateVerificationStatus(int id, bool isVerified)
        {
            var pending = _context.PendingUserRegistrations.FirstOrDefault(p => p.Id == id);
            if (pending != null)
            {
                pending.IsVerified = isVerified;
                pending.UpdatedTime = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public void DeletePending(int id)
        {
            var pending = _context.PendingUserRegistrations.FirstOrDefault(p => p.Id == id);
            if (pending != null)
            {
                _context.PendingUserRegistrations.Remove(pending);
                _context.SaveChanges();
            }
        }

        public void CleanupExpiredTokens()
        {
            var expiredTokens = _context.PendingUserRegistrations
                .Where(p => p.TokenExpiration < DateTime.Now)
                .ToList();

            _context.PendingUserRegistrations.RemoveRange(expiredTokens);
            _context.SaveChanges();
        }

        public bool EmailExists(string emailId)
        {
            return _context.PendingUserRegistrations
                .Any(p => p.EmailId == emailId);
        }
    }
}

