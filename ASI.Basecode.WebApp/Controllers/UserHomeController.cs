using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// User Home Controller
    /// </summary>
    public class UserHomeController : ControllerBase<UserHomeController>
    {
        private readonly IOrgBooksService _booksService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="booksService"></param>
        /// <param name="mapper"></param>
        public UserHomeController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IOrgBooksService booksService,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _booksService = booksService;
        }

        /// <summary>
        /// Returns User Home View.
        /// </summary>
        /// <returns> User Home View </returns>
        public IActionResult Index()
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
            
            return View(books);
        }
    }
}

