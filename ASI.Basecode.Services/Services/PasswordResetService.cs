using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IPasswordResetTokenRepository _tokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PasswordResetService(
            IPasswordResetTokenRepository tokenRepository,
            IUserRepository userRepository,
            IAdminRepository adminRepository,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _tokenRepository = tokenRepository;
            _userRepository = userRepository;
            _adminRepository = adminRepository;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> SendPasswordResetEmail(string email, int role)
        {
            try
            {
                string userName = string.Empty;
                bool userExists = false;

                // Check if user/admin exists based on role
                if (role == 0) // User
                {
                    var user = _userRepository.GetUsers().FirstOrDefault(u => u.Email == email);
                    if (user != null)
                    {
                        userExists = true;
                        userName = user.Name;
                    }
                }
                else if (role == 1) // Admin
                {
                    var admin = _adminRepository.GetAdmins().FirstOrDefault(a => a.Email == email);
                    if (admin != null)
                    {
                        userExists = true;
                        userName = admin.AdminUsername;
                    }
                }

                // Even if user doesn't exist, we proceed to avoid revealing account existence
                // But we only actually send the email if the user exists
                if (userExists)
                {
                    // Generate unique token
                    string token = Guid.NewGuid().ToString();

                    // Create password reset token record
                    var resetToken = new PasswordResetToken
                    {
                        Email = email,
                        Role = role,
                        Token = token,
                        ExpirationTime = DateTime.Now.AddMinutes(30),
                        IsUsed = false,
                        CreatedTime = DateTime.Now
                    };

                    _tokenRepository.AddToken(resetToken);

                    // Generate reset link
                    var request = _httpContextAccessor.HttpContext.Request;
                    var baseUrl = $"{request.Scheme}://{request.Host}";
                    var resetAction = role == 0 ? "ResetPasswordUser" : "ResetPasswordAdmin";
                    var resetLink = $"{baseUrl}/Account/{resetAction}?token={token}";

                    // Send email
                    await _emailService.SendPasswordResetEmail(email, userName, resetLink);
                }

                return true; // Always return true for security (don't reveal if email exists)
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Password reset service error: {ex.Message}");
                return false;
            }
        }

        public bool ValidateResetToken(string token)
        {
            var resetToken = _tokenRepository.GetByToken(token);
            return resetToken != null;
        }

        public void ResetPassword(string token, string newPassword)
        {
            var resetToken = _tokenRepository.GetByToken(token);

            if (resetToken == null)
            {
                throw new InvalidOperationException("Invalid or expired reset token.");
            }

            // Encrypt the new password
            string encryptedPassword = PasswordManager.EncryptPassword(newPassword);

            // Update password based on role
            if (resetToken.Role == 0) // User
            {
                var user = _userRepository.GetUsers().FirstOrDefault(u => u.Email == resetToken.Email);
                if (user != null)
                {
                    user.Password = encryptedPassword;
                    _userRepository.UpdateUser(user);
                }
            }
            else if (resetToken.Role == 1) // Admin
            {
                var admin = _adminRepository.GetAdmins().FirstOrDefault(a => a.Email == resetToken.Email);
                if (admin != null)
                {
                    admin.AdminPassword = encryptedPassword;
                    _adminRepository.UpdateAdmin(admin);
                }
            }

            // Mark token as used
            _tokenRepository.MarkAsUsed(resetToken.TokenId);
        }
    }
}

