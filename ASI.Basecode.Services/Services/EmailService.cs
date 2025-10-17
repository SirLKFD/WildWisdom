using ASI.Basecode.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ASI.Basecode.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _resendApiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _emailTemplatePath;
        private readonly string _passwordResetTemplatePath;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _resendApiKey = _configuration["EmailSettings:ResendApiKey"];
            _fromEmail = _configuration["EmailSettings:FromEmail"];
            _fromName = _configuration["EmailSettings:FromName"];
            _emailTemplatePath = _configuration["EmailSettings:TemplatePath"];
            _passwordResetTemplatePath = _configuration["EmailSettings:PasswordResetTemplatePath"];
        }

        public async Task<bool> SendOTPEmail(string toEmail, string toName, string otp, string verificationLink)
        {
            try
            {
                // Read the email template
                string htmlTemplate = File.ReadAllText(_emailTemplatePath);

                // Replace placeholders in the template
                htmlTemplate = htmlTemplate.Replace("{{UserName}}", toName);
                htmlTemplate = htmlTemplate.Replace("{{OTP}}", otp);
                htmlTemplate = htmlTemplate.Replace("{{VerificationLink}}", verificationLink);
                htmlTemplate = htmlTemplate.Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

                // Prepare the email request for Resend API
                var emailRequest = new
                {
                    from = $"{_fromName} <{_fromEmail}>",
                    to = new[] { toEmail },
                    subject = "Verify Your WildWisdom Account",
                    html = htmlTemplate
                };

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_resendApiKey}");

                    var content = new StringContent(
                        JsonConvert.SerializeObject(emailRequest),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await httpClient.PostAsync("https://api.resend.com/emails", content);

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application, use a proper logging framework)
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmail(string toEmail, string userName, string resetLink)
        {
            try
            {
                // Read the password reset email template
                string htmlTemplate = File.ReadAllText(_passwordResetTemplatePath);

                // Replace placeholders in the template
                htmlTemplate = htmlTemplate.Replace("{{UserName}}", userName);
                htmlTemplate = htmlTemplate.Replace("{{ResetLink}}", resetLink);
                htmlTemplate = htmlTemplate.Replace("{{ExpirationMinutes}}", "30");
                htmlTemplate = htmlTemplate.Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());

                // Prepare the email request for Resend API
                var emailRequest = new
                {
                    from = $"{_fromName} <{_fromEmail}>",
                    to = new[] { toEmail },
                    subject = "Reset Your WildWisdom Password",
                    html = htmlTemplate
                };

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_resendApiKey}");

                    var content = new StringContent(
                        JsonConvert.SerializeObject(emailRequest),
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await httpClient.PostAsync("https://api.resend.com/emails", content);

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Password reset email sending failed: {ex.Message}");
                return false;
            }
        }
    }
}

