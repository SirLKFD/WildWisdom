using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IPasswordResetService
    {
        Task<bool> SendPasswordResetEmail(string email, int role);
        bool ValidateResetToken(string token);
        void ResetPassword(string token, string newPassword);
    }
}

