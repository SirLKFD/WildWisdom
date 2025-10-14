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
                // Authentication failed
                TempData["ErrorMessage"] = "Incorrect UserId or Password";
                return View();
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
        public IActionResult RegisterUser(UserViewModel model)
        {
            try
            {
                _userService.AddUser(model);
                return RedirectToAction("LoginUser", "Account");
            }
            catch(InvalidDataException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
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
                // Authentication failed
                TempData["ErrorMessage"] = "Incorrect Admin Username or Password";
                return View();
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
        public IActionResult RegisterAdmin(AdminViewModel model)
        {
            try
            {
                _adminService.AddAdmin(model);
                return RedirectToAction("LoginAdmin", "Account");
            }
            catch(InvalidDataException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = Resources.Messages.Errors.ServerError;
            }
            return View();
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
    }
}
