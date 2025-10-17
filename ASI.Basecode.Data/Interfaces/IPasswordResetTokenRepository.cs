using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        void AddToken(PasswordResetToken token);
        PasswordResetToken GetByToken(string token);
        void MarkAsUsed(int tokenId);
        void DeleteExpiredTokens();
    }
}

