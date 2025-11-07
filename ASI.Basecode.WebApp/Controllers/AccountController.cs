using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Models;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    public class AccountController : ControllerBase<AccountController>
    {
        private readonly SessionManager _sessionManager;
        private readonly SignInManager _signInManager;
        private readonly TokenValidationParametersFactory _tokenValidationParametersFactory;
        private readonly TokenProviderOptionsFactory _tokenProviderOptionsFactory;
        private readonly IConfiguration _appConfiguration;
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;
        private readonly IOrganizationService _organizationService;
        private readonly IEmailService _emailService;
        private readonly IOTPService _otpService;
        private readonly IPendingUserRegistrationRepository _pendingRepository;
        private readonly IPasswordResetService _passwordResetService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="localizer">The localizer.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public AccountController(
                            SignInManager signInManager,
                            IHttpContextAccessor httpContextAccessor,
                            ILoggerFactory loggerFactory,
                            IConfiguration configuration,
                            IMapper mapper,
                            IUserService userService,
                            IAdminService adminService,
                            IOrganizationService organizationService,
                            IEmailService emailService,
                            IOTPService otpService,
                            IPendingUserRegistrationRepository pendingRepository,
                            IPasswordResetService passwordResetService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._sessionManager = new SessionManager(this._session);
            this._signInManager = signInManager;
            this._tokenProviderOptionsFactory = tokenProviderOptionsFactory;
            this._tokenValidationParametersFactory = tokenValidationParametersFactory;
            this._appConfiguration = configuration;
            this._userService = userService;
            this._adminService = adminService;
            this._organizationService = organizationService;
            this._emailService = emailService;
            this._otpService = otpService;
            this._pendingRepository = pendingRepository;
            this._passwordResetService = passwordResetService;
        }

        /// <summary>
        /// User Login Method
        /// </summary>
        /// <returns>Created response view</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult LoginUser()
        {
            // Redirect if already logged in
            var hasSession = this._session.GetString("HasSession");
            var userRole = this._session.GetString("UserRole");
            
            if (!string.IsNullOrEmpty(hasSession))
            {
                if (userRole == "Admin")
                {
                    return RedirectToAction("Index", "AdminHome");
                }
                else if (userRole == "User")
                {
                    return RedirectToAction("Index", "UserHome");
                }
            }
            
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return this.View();
        }

        /// <summary>
        /// Authenticate user and signs the user in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            this._session.SetString("HasSession", "Exist");

            User user = new User();

            var loginResult = _userService.AuthenticateUser(model.UserId, model.Password, ref user);
            if (loginResult == LoginResult.Success)
            {
                // Authentication successful
                await this._signInManager.SignInAsync(user);
                this._session.SetString("UserName", user.Name);
                this._session.SetString("UserId", user.UserId); // Store UserId for user lookup
                this._session.SetInt32("UserRole", 0); // 0 = User
                this._session.SetInt32("OrganizationId", user.OrganizationId ?? 0);
                
                // Store profile picture in session
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    this._session.SetString("ProfilePicture", user.ProfilePicture);
                }
                
                return RedirectToAction("Index", "UserHome");
            }
            else
            {
                // Authentication failed - use ModelState for security (don't reveal if account exists)
                ModelState.AddModelError(string.Empty, "Incorrect username/email or password");
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterUser()
        {
            // Redirect if already logged in
            var hasSession = this._session.GetString("HasSession");
            var userRole = this._session.GetString("UserRole");
            
            if (!string.IsNullOrEmpty(hasSession))
            {
                if (userRole == "Admin")
                {
                    return RedirectToAction("Index", "AdminHome");
                }
                else if (userRole == "User")
                {
                    return RedirectToAction("Index", "UserHome");
                }
            }
            
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Generate OTP and verification token
                string otp = _otpService.GenerateOTP();
                string encryptedOTP = _otpService.EncryptOTP(otp);
                string verificationToken = _otpService.GenerateVerificationToken();

                // Create pending user registration
                _userService.CreatePendingUser(model, encryptedOTP, verificationToken);

                // Generate verification link
                string verificationLink = Url.Action("OTPVerificationUser", "Account", 
                    new { email = model.Email, token = verificationToken }, Request.Scheme);

                // Send email with OTP
                bool emailSent = await _emailService.SendOTPEmail(model.Email, model.Name, otp, verificationLink);

                if (!emailSent)
                {
                    ModelState.AddModelError(string.Empty, "Failed to send verification email. Please try again.");
                    return View(model);
                }

                // Store email in TempData for modal
                TempData["RegistrationEmail"] = model.Email;
                TempData["ShowVerificationModal"] = true;
                
                return View(model);
            }
            catch(InvalidDataException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, Resources.Messages.Errors.ServerError);
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            // Redirect if already logged in
            var hasSession = this._session.GetString("HasSession");
            var userRole = this._session.GetString("UserRole");
            
            if (!string.IsNullOrEmpty(hasSession))
            {
                if (userRole == "Admin")
                {
                    return RedirectToAction("Index", "AdminHome");
                }
                else if (userRole == "User")
                {
                    return RedirectToAction("Index", "UserHome");
                }
            }
            
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Generate OTP and verification token
                string otp = _otpService.GenerateOTP();
                string encryptedOTP = _otpService.EncryptOTP(otp);
                string verificationToken = _otpService.GenerateVerificationToken();

                // Create pending user registration
                _userService.CreatePendingUser(model, encryptedOTP, verificationToken);

                // Generate verification link
                string verificationLink = Url.Action("OTPVerificationUser", "Account", 
                    new { email = model.Email, token = verificationToken }, Request.Scheme);

                // Send email with OTP
                bool emailSent = await _emailService.SendOTPEmail(model.Email, model.Name, otp, verificationLink);

                if (!emailSent)
                {
                    ModelState.AddModelError(string.Empty, "Failed to send verification email. Please try again.");
                    return View(model);
                }

                // Store email in TempData for modal
                TempData["RegistrationEmail"] = model.Email;
                TempData["ShowVerificationModal"] = true;
                
                return View(model);
            }
            catch(InvalidDataException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, Resources.Messages.Errors.ServerError);
            }
            return View(model);
        }

        /// <summary>
        /// Admin Login Method
        /// </summary>
        /// <returns>Created response view</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult LoginAdmin()
        {
            // Redirect if already logged in
            var hasSession = this._session.GetString("HasSession");
            var userRole = this._session.GetString("UserRole");
            
            if (!string.IsNullOrEmpty(hasSession))
            {
                if (userRole == "Admin")
                {
                    return RedirectToAction("Index", "AdminHome");
                }
                else if (userRole == "User")
                {
                    return RedirectToAction("Index", "UserHome");
                }
            }
            
            TempData["returnUrl"] = System.Net.WebUtility.UrlDecode(HttpContext.Request.Query["ReturnUrl"]);
            this._sessionManager.Clear();
            this._session.SetString("SessionId", System.Guid.NewGuid().ToString());
            return this.View();
        }

        /// <summary>
        /// Authenticate admin and signs the admin in when successful.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns> Created response view </returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAdmin(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            this._session.SetString("HasSession", "Exist");

            Data.Models.Admin admin = new Data.Models.Admin();

            var loginResult = _adminService.AuthenticateAdmin(model.UserId, model.Password, ref admin);
            if (loginResult == LoginResult.Success)
            {
                // Authentication successful
                // Create a user object for sign-in (reusing existing infrastructure)
                var user = new User
                { 
                    Id = admin.Id, 
                    UserId = admin.AdminUsername, 
                    Name = admin.AdminUsername, 
                    Password = admin.AdminPassword,
                    Role = 1
                };
                await this._signInManager.SignInAsync(user);
                this._session.SetString("UserName", admin.AdminUsername);
                this._session.SetInt32("UserId", admin.Id); // Store admin's database ID
                this._session.SetInt32("UserRole", 1); // 1 = Admin
                this._session.SetInt32("OrganizationId", admin.OrganizationId ?? 0);
                
                // Store organization name in session
                if (admin.OrganizationId.HasValue)
                {
                    var organization = _organizationService.GetOrganizationById(admin.OrganizationId.Value);
                    if (organization != null)
                    {
                        this._session.SetString("OrganizationName", organization.Name);
                    }
                }
                
                // Store profile picture in session
                if (!string.IsNullOrEmpty(admin.ProfilePicture))
                {
                    this._session.SetString("ProfilePicture", admin.ProfilePicture);
                }
                
                return RedirectToAction("Dashboard", "AdminHome");
            }
            else
            {
                // Authentication failed - use ModelState for security (don't reveal if account exists)
                ModelState.AddModelError(string.Empty, "Incorrect username/email or password");
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterAdmin()
        {
            // Redirect if already logged in
            var hasSession = this._session.GetString("HasSession");
            var userRole = this._session.GetString("UserRole");
            
            if (!string.IsNullOrEmpty(hasSession))
            {
                if (userRole == "Admin")
                {
                    return RedirectToAction("Index", "AdminHome");
                }
                else if (userRole == "User")
                {
                    return RedirectToAction("Index", "UserHome");
                }
            }
            
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAdmin(AdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Generate OTP and verification token
                string otp = _otpService.GenerateOTP();
                string encryptedOTP = _otpService.EncryptOTP(otp);
                string verificationToken = _otpService.GenerateVerificationToken();

                // Create pending admin registration
                _adminService.CreatePendingAdmin(model, encryptedOTP, verificationToken);

                // Generate verification link
                string verificationLink = Url.Action("OTPVerificationAdmin", "Account", 
                    new { email = model.Email, token = verificationToken }, Request.Scheme);

                // Send email with OTP
                bool emailSent = await _emailService.SendOTPEmail(model.Email, model.AdminUsername, otp, verificationLink);

                if (!emailSent)
                {
                    ModelState.AddModelError(string.Empty, "Failed to send verification email. Please try again.");
                    return View(model);
                }

                // Store email in TempData for modal
                TempData["RegistrationEmail"] = model.Email;
                TempData["ShowVerificationModal"] = true;
                
                return View(model);
            }
            catch(InvalidDataException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch(Exception ex)
            {
                ModelState.AddModelError(string.Empty, Resources.Messages.Errors.ServerError);
            }
            return View(model);
        }

        /// <summary>
        /// Sign Out current account and return login view.
        /// </summary>
        /// <returns>Created response view</returns>
        [AllowAnonymous]
        public async Task<IActionResult> SignOutUser()
        {
            // Get the user role before signing out
            var userRole = this._session.GetInt32("UserRole");
            
            await this._signInManager.SignOutAsync();
            this._sessionManager.Clear();
            
            // Redirect based on role (1 = Admin, 0 = User)
            if (userRole == 1)
            {
                return RedirectToAction("LoginAdmin", "Account");
            }
            else
            {
                return RedirectToAction("LoginUser", "Account");
            }
        }

        #region User Authentication Pages

        // User Forgot Password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordUser()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordUser(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Send password reset email (service handles if email doesn't exist)
            await _passwordResetService.SendPasswordResetEmail(model.Email, 0); // 0 = User

            // Show modal with success message
            TempData["ShowEmailSentModal"] = true;
            TempData["ResetEmail"] = model.Email;
            return View(model);
        }

        // User Reset Password (from email link)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordUser(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid reset link.";
                return RedirectToAction("ForgotPasswordUser");
            }

            if (!_passwordResetService.ValidateResetToken(token))
            {
                TempData["ErrorMessage"] = "Invalid or expired reset link. Please request a new one.";
                return RedirectToAction("ForgotPasswordUser");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ResetPasswordUser(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Token = model.Token;
                return View(model);
            }

            try
            {
                _passwordResetService.ResetPassword(model.Token, model.NewPassword);
                TempData["SuccessMessage"] = "Password reset successfully! Please login with your new password.";
                return RedirectToAction("LoginUser");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                ViewBag.Token = model.Token;
                return View(model);
            }
        }

        // User Change Password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ChangePasswordUser(string token, string email)
        {
            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ChangePasswordUser(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Token = model.Token;
                ViewBag.Email = model.Email;
                return View(model);
            }

            try
            {
                // Validate token and update password
                _passwordResetService.ResetPassword(model.Token, model.NewPassword);
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("LoginUser");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Token = model.Token;
                ViewBag.Email = model.Email;
                return View(model);
            }
        }

        // User OTP Verification
        [HttpGet]
        [AllowAnonymous]
        public IActionResult OTPVerificationUser(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid verification link.";
                return RedirectToAction("RegisterUser");
            }

            // Check if pending registration exists and token is valid
            var pending = _pendingRepository.GetByEmail(email);
            if (pending == null)
            {
                TempData["ErrorMessage"] = "Pending registration not found. Please register again.";
                return RedirectToAction("RegisterUser");
            }

            if (pending.TokenExpiration < DateTime.Now)
            {
                _pendingRepository.DeletePending(pending.Id);
                TempData["ErrorMessage"] = "Verification link has expired. Please register again.";
                return RedirectToAction("RegisterUser");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult VerifyOTPUser(string email, string otpCode, string token)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otpCode) || string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                ViewBag.Email = email;
                ViewBag.Token = token;
                return View("OTPVerificationUser");
            }

            // Validate OTP format (should be 6 digits)
            if (!System.Text.RegularExpressions.Regex.IsMatch(otpCode, @"^\d{6}$"))
            {
                ModelState.AddModelError(string.Empty, "OTP code must be 6 digits.");
                ViewBag.Email = email;
                ViewBag.Token = token;
                return View("OTPVerificationUser");
            }

            try
            {
                var pending = _pendingRepository.GetByEmail(email);
                if (pending == null)
                {
                    ModelState.AddModelError(string.Empty, "Pending registration not found. Please register again.");
                    ViewBag.Email = email;
                    ViewBag.Token = token;
                    return View("OTPVerificationUser");
                }

                // Validate OTP
                if (!_otpService.ValidateOTP(otpCode, pending.EncryptedOTP))
                {
                    ModelState.AddModelError(string.Empty, "Invalid OTP code. Please try again.");
                    ViewBag.Email = email;
                    ViewBag.Token = token;
                    return View("OTPVerificationUser");
                }

                // Create user account
                _userService.VerifyAndCreateUser(email, token);
                
                TempData["SuccessMessage"] = "Account verified successfully! You can now login.";
                return RedirectToAction("LoginUser");
            }
            catch (InvalidDataException ex)
            {
                // Only redirect to RegisterUser if it's a critical error (like expired token)
                if (ex.Message.Contains("expired") || ex.Message.Contains("not found"))
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToAction("RegisterUser");
                }
                else
                {
                    // For other validation errors, stay on OTP page
                    ModelState.AddModelError(string.Empty, ex.Message);
                    ViewBag.Email = email;
                    ViewBag.Token = token;
                    return View("OTPVerificationUser");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred during verification. Please try again.");
                ViewBag.Email = email;
                ViewBag.Token = token;
                return View("OTPVerificationUser");
            }
        }

        #endregion

        #region Admin Authentication Pages

        // Admin Forgot Password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordAdmin()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordAdmin(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Send password reset email (service handles if email doesn't exist)
            await _passwordResetService.SendPasswordResetEmail(model.Email, 1); // 1 = Admin

            // Show modal with success message
            TempData["ShowEmailSentModal"] = true;
            TempData["ResetEmail"] = model.Email;
            return View(model);
        }

        // Admin Reset Password (from email link)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordAdmin(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid reset link.";
                return RedirectToAction("ForgotPasswordAdmin");
            }

            if (!_passwordResetService.ValidateResetToken(token))
            {
                TempData["ErrorMessage"] = "Invalid or expired reset link. Please request a new one.";
                return RedirectToAction("ForgotPasswordAdmin");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ResetPasswordAdmin(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Token = model.Token;
                return View(model);
            }

            try
            {
                _passwordResetService.ResetPassword(model.Token, model.NewPassword);
                TempData["SuccessMessage"] = "Password reset successfully! Please login with your new password.";
                return RedirectToAction("LoginAdmin");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                ViewBag.Token = model.Token;
                return View(model);
            }
        }

        // Admin Change Password
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ChangePasswordAdmin(string token, string email)
        {
            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ChangePasswordAdmin(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Token = model.Token;
                ViewBag.Email = model.Email;
                return View(model);
            }

            try
            {
                // Validate token and update password
                _passwordResetService.ResetPassword(model.Token, model.NewPassword);
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("LoginAdmin");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Token = model.Token;
                ViewBag.Email = model.Email;
                return View(model);
            }
        }

        // Admin OTP Verification
        [HttpGet]
        [AllowAnonymous]
        public IActionResult OTPVerificationAdmin(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Invalid verification link.";
                return RedirectToAction("RegisterAdmin");
            }

            // Check if pending registration exists and token is valid
            var pending = _pendingRepository.GetByEmail(email);
            if (pending == null)
            {
                TempData["ErrorMessage"] = "Pending registration not found. Please register again.";
                return RedirectToAction("RegisterAdmin");
            }

            if (pending.TokenExpiration < DateTime.Now)
            {
                _pendingRepository.DeletePending(pending.Id);
                TempData["ErrorMessage"] = "Verification link has expired. Please register again.";
                return RedirectToAction("RegisterAdmin");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult VerifyOTPAdmin(string email, string otpCode, string token)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otpCode) || string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                ViewBag.Email = email;
                ViewBag.Token = token;
                return View("OTPVerificationAdmin");
            }

            // Validate OTP format (should be 6 digits)
            if (!System.Text.RegularExpressions.Regex.IsMatch(otpCode, @"^\d{6}$"))
            {
                ModelState.AddModelError(string.Empty, "OTP code must be 6 digits.");
                ViewBag.Email = email;
                ViewBag.Token = token;
                return View("OTPVerificationAdmin");
            }

            try
            {
                var pending = _pendingRepository.GetByEmail(email);
                if (pending == null)
                {
                    ModelState.AddModelError(string.Empty, "Pending registration not found. Please register again.");
                    ViewBag.Email = email;
                    ViewBag.Token = token;
                    return View("OTPVerificationAdmin");
                }

                // Validate OTP
                if (!_otpService.ValidateOTP(otpCode, pending.EncryptedOTP))
                {
                    ModelState.AddModelError(string.Empty, "Invalid OTP code. Please try again.");
                    ViewBag.Email = email;
                    ViewBag.Token = token;
                    return View("OTPVerificationAdmin");
                }

                // Create admin account
                _adminService.VerifyAndCreateAdmin(email, token);
                
                TempData["SuccessMessage"] = "Account verified successfully! You can now login.";
                return RedirectToAction("LoginAdmin");
            }
            catch (InvalidDataException ex)
            {
                // Only redirect to RegisterAdmin if it's a critical error (like expired token)
                if (ex.Message.Contains("expired") || ex.Message.Contains("not found"))
                {
                    TempData["ErrorMessage"] = ex.Message;
                    return RedirectToAction("RegisterAdmin");
                }
                else
                {
                    // For other validation errors, stay on OTP page
                    ModelState.AddModelError(string.Empty, ex.Message);
                    ViewBag.Email = email;
                    ViewBag.Token = token;
                    return View("OTPVerificationAdmin");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred during verification. Please try again.");
                ViewBag.Email = email;
                ViewBag.Token = token;
                return View("OTPVerificationAdmin");
            }
        }

        #endregion
    }
}
