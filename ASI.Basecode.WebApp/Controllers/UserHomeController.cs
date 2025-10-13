using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// User Home Controller
    /// </summary>
    public class UserHomeController : ControllerBase<UserHomeController>
    {
        private readonly IOrgBooksService _booksService;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="booksService"></param>
        /// <param name="userService"></param>
        /// <param name="organizationService"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="mapper"></param>
        public UserHomeController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IOrgBooksService booksService,
                              IUserService userService,
                              IOrganizationService organizationService,
                              IWebHostEnvironment webHostEnvironment,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _booksService = booksService;
            _userService = userService;
            _organizationService = organizationService;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Returns User Home View.
        /// </summary>
        /// <param name="search">Search term</param>
        /// <param name="category">Category filter</param>
        /// <param name="genre">Genre filter</param>
        /// <param name="availability">Availability filter</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns> User Home View </returns>
        public IActionResult Index(string search = "", string category = "", string genre = "", string availability = "", int page = 1, int pageSize = 25)
        {
            // Check if user is logged in
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("LoginUser", "Account");
            }

            // Check if user has the correct role (0 = User)
            var userRole = this._session.GetInt32("UserRole");
            if (userRole == 1) // Explicitly check if Admin
            {
                // User is logged in as Admin, redirect to AdminHome
                TempData["ErrorMessage"] = "Access denied. You are logged in as an Administrator.";
                return RedirectToAction("Index", "AdminHome");
            }

            // Fetch active books from user's organization
            var orgId = this._session.GetInt32("OrganizationId") ?? 0;
            var books = _booksService.GetBookCatalog(orgId);

            // Get distinct categories and genres for filters
            var categories = books.Where(b => !string.IsNullOrEmpty(b.Category))
                                  .Select(b => b.Category)
                                  .Distinct()
                                  .OrderBy(c => c)
                                  .ToList();
            
            var genres = books.Where(b => !string.IsNullOrEmpty(b.Genre))
                             .Select(b => b.Genre)
                             .Distinct()
                             .OrderBy(g => g)
                             .ToList();

            ViewBag.Categories = categories;
            ViewBag.Genres = genres;
            ViewBag.SearchTerm = search;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedGenre = genre;
            ViewBag.SelectedAvailability = availability;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            
            return View(books);
        }

        /// <summary>
        /// Returns Browse Books View.
        /// </summary>
        /// <param name="search">Search term</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns> Browse View </returns>
        public IActionResult Browse(string search = "", int page = 1, int pageSize = 10)
        {
            // Check if user is logged in
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("LoginUser", "Account");
            }

            // Fetch active books from user's organization
            var orgId = this._session.GetInt32("OrganizationId") ?? 0;
            var books = _booksService.GetBookCatalog(orgId);

            ViewBag.SearchTerm = search;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            
            return View(books);
        }

        /// <summary>
        /// Returns Popular Books View.
        /// </summary>
        /// <param name="search">Search term</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns> Popular View </returns>
        public IActionResult Popular(string search = "", int page = 1, int pageSize = 10)
        {
            // Check if user is logged in
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("LoginUser", "Account");
            }

            // Fetch active books from user's organization
            var orgId = this._session.GetInt32("OrganizationId") ?? 0;
            var books = _booksService.GetBookCatalog(orgId);

            ViewBag.SearchTerm = search;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            
            return View(books);
        }

        /// <summary>
        /// Returns Book Recommendations View.
        /// </summary>
        /// <param name="search">Search term</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns> BookRecommendations View </returns>
        public IActionResult BookRecommendations(string search = "", int page = 1, int pageSize = 10)
        {
            // Check if user is logged in
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("LoginUser", "Account");
            }

            // Fetch active books from user's organization
            var orgId = this._session.GetInt32("OrganizationId") ?? 0;
            var books = _booksService.GetBookCatalog(orgId);

            ViewBag.SearchTerm = search;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            
            return View(books);
        }

        /// <summary>
        /// Book Checkout Action
        /// </summary>
        /// <returns> BookCheckout View </returns>
        public IActionResult BookCheckout()
        {
            // Check if user is logged in
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("LoginUser", "Account");
            }
            
            return View();
        }

        /// <summary>
        /// User Settings Action - GET
        /// </summary>
        /// <returns> UserSettings View </returns>
        [HttpGet]
        public IActionResult UserSettings()
        {
            // Check if user is logged in
            var userId = this._session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("LoginUser", "Account");
            }

            // Get user data
            var userModel = _userService.GetUserByUserId(userId);
            if (userModel == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            // Get organizations for dropdown
            var organizations = _organizationService.GetAllOrganizations();
            ViewBag.Organizations = organizations;

            return View(userModel);
        }

        /// <summary>
        /// User Settings Action - POST
        /// </summary>
        /// <param name="model">User settings model</param>
        /// <returns> Redirect or View with validation errors </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UserSettings(UserSettingsViewModel model)
        {
            var userId = this._session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("LoginUser", "Account");
            }

            try
            {
                // Validate password change if requested
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    if (string.IsNullOrEmpty(model.OldPassword))
                    {
                        ModelState.AddModelError("OldPassword", "Current password is required to change password.");
                        ViewBag.Organizations = _organizationService.GetAllOrganizations();
                        return View(model);
                    }

                    // Verify old password
                    var user = _userService.GetUserByUserId(userId);
                    var passwordKey = ASI.Basecode.Services.Manager.PasswordManager.EncryptPassword(model.OldPassword);
                    var currentUser = _userService.GetUserByUserId(userId);
                    
                    // Get the actual user from repository to check password
                    var userEntity = new Data.Models.User();
                    var loginResult = _userService.AuthenticateUser(userId, model.OldPassword, ref userEntity);
                    
                    if (loginResult != ASI.Basecode.Resources.Constants.Enums.LoginResult.Success)
                    {
                        ModelState.AddModelError("OldPassword", "Current password is incorrect.");
                        ViewBag.Organizations = _organizationService.GetAllOrganizations();
                        return View(model);
                    }
                }

                // Handle profile picture upload
                if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{model.ProfilePictureFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ProfilePictureFile.CopyTo(fileStream);
                    }

                    model.ProfilePicture = $"/uploads/profiles/{uniqueFileName}";

                    // Update session with new profile picture
                    this._session.SetString("ProfilePicture", model.ProfilePicture);
                }

                // Update user
                _userService.UpdateUser(model, userId);

                // Update session if userId changed
                if (model.UserId != userId)
                {
                    this._session.SetString("UserId", model.UserId);
                }
                
                // Update session with new name
                this._session.SetString("UserName", model.Name);

                TempData["SuccessMessage"] = "Settings updated successfully!";
                return RedirectToAction("UserSettings");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Organizations = _organizationService.GetAllOrganizations();
                return View(model);
            }
        }

        /// <summary>
        /// Delete User Account
        /// </summary>
        /// <returns> Redirect to Landing Page </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAccount()
        {
            var userId = this._session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("LoginUser", "Account");
            }

            try
            {
                var userModel = _userService.GetUserByUserId(userId);
                if (userModel != null)
                {
                    _userService.DeleteUser(userModel.Id);
                    
                    // Clear session
                    HttpContext.Session.Clear();
                    
                    TempData["SuccessMessage"] = "Your account has been deleted successfully.";
                    return RedirectToAction("Index", "LandingPage");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting account: {ex.Message}";
            }

            return RedirectToAction("UserSettings");
        }
    }
}

