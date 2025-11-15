using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendOTPEmail(string toEmail, string toName, string otp, string verificationLink);
        Task<bool> SendPasswordResetEmail(string toEmail, string userName, string resetLink);
    }
}

