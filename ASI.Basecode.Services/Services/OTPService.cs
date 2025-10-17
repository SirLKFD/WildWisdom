using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using System;

namespace ASI.Basecode.Services.Services
{
    public class OTPService : IOTPService
    {
        public string GenerateOTP()
        {
            // Generate a 6-digit OTP
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public string EncryptOTP(string otp)
        {
            // Use the existing PasswordManager to encrypt the OTP
            return PasswordManager.EncryptPassword(otp);
        }

        public bool ValidateOTP(string inputOTP, string storedEncryptedOTP)
        {
            // Encrypt the input OTP and compare with stored encrypted OTP
            string encryptedInput = EncryptOTP(inputOTP);
            return encryptedInput == storedEncryptedOTP;
        }

        public string GenerateVerificationToken()
        {
            // Generate a GUID-based verification token
            return Guid.NewGuid().ToString();
        }
    }
}

