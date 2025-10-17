using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class PasswordResetTokenRepository : BaseRepository<PasswordResetToken>, IPasswordResetTokenRepository
    {
        public PasswordResetTokenRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void AddToken(PasswordResetToken token)
        {
            this.GetDbSet<PasswordResetToken>().Add(token);
            UnitOfWork.SaveChanges();
        }

        public PasswordResetToken GetByToken(string token)
        {
            return this.GetDbSet<PasswordResetToken>()
                .FirstOrDefault(t => t.Token == token && 
                                    !t.IsUsed && 
                                    t.ExpirationTime > DateTime.Now);
        }

        public void MarkAsUsed(int tokenId)
        {
            var token = this.GetDbSet<PasswordResetToken>().Find(tokenId);
            if (token != null)
            {
                token.IsUsed = true;
                UnitOfWork.SaveChanges();
            }
        }

        public void DeleteExpiredTokens()
        {
            var expiredTokens = this.GetDbSet<PasswordResetToken>()
                .Where(t => t.ExpirationTime < DateTime.Now)
                .ToList();

            foreach (var token in expiredTokens)
            {
                this.GetDbSet<PasswordResetToken>().Remove(token);
            }

            if (expiredTokens.Any())
            {
                UnitOfWork.SaveChanges();
            }
        }
    }
}

