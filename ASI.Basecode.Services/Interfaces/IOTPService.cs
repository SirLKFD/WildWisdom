namespace ASI.Basecode.Services.Interfaces
{
    public interface IOTPService
    {
        string GenerateOTP();
        string EncryptOTP(string otp);
        bool ValidateOTP(string inputOTP, string storedEncryptedOTP);
        string GenerateVerificationToken();
    }
}

