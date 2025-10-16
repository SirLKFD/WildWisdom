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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

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
        private readonly IBorrowTicketService _borrowTicketService;
        private readonly IBookReviewService _reviewService;
        private readonly IBookCheckoutService _bookCheckoutService;
        private readonly IWishlistService _wishlistService;
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
        /// <param name="reviewService"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="mapper"></param>
        public UserHomeController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IOrgBooksService booksService,
                              IUserService userService,
                              IOrganizationService organizationService,
                              IBorrowTicketService borrowTicketService,
                              IBookReviewService reviewService,
                              IBookCheckoutService bookCheckoutService,
                              IWishlistService wishlistService,
                              IWebHostEnvironment webHostEnvironment,
                              IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _booksService = booksService;
            _userService = userService;
            _organizationService = organizationService;
            _borrowTicketService = borrowTicketService;
            _reviewService = reviewService;
            _bookCheckoutService = bookCheckoutService;
            _wishlistService = wishlistService;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Returns User Home View.
        /// </summary>
        /// <param name="search">Search term</param>
        /// <param name="category">Category filter</param>
        /// <param name="genre">Genre filter</param>
        /// <param name="availability">Availability filter</param>
        /// <param name="sortBy">Sort order</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns> User Home View </returns>
        public IActionResult Index(string search = "", string category = "", string genre = "", string availability = "", string sortBy = "", int page = 1, int pageSize = 25)
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
            ViewBag.SortBy = sortBy;
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

            var userId = this._session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("LoginUser", "Account");
            }

            var userModel = _userService.GetUserByUserId(userId);
            if (userModel == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Index");
            }

            var orgId = this._session.GetInt32("OrganizationId") ?? 0;
            var tickets = _borrowTicketService.GetUserTickets(userModel.Id) ?? new List<BorrowTicketViewModel>();

            var pendingTickets = tickets
                .Where(t => t.Status == TicketStatus.NotYetClaimed)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            var borrowedTickets = tickets
                .Where(t => t.Status == TicketStatus.NotYetReturned)
                .OrderByDescending(t => t.CheckedOutAt ?? t.CreatedAt)
                .ToList();

            var historyTickets = tickets
                .Where(t => t.Status == TicketStatus.Returned
                         || t.Status == TicketStatus.Cancelled
                         || t.Status == TicketStatus.Expired
                         || t.Status == TicketStatus.Lost
                         || t.Status == TicketStatus.Claimed)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            // Get cart items from database
            var cartItems = _bookCheckoutService.GetUserCart(userModel.Id)
                .Select(item => new CartItemViewModel
                {
                    BookId = item.BookId,
                    BookTitle = item.BookTitle,
                    BookAuthor = item.BookAuthor,
                    BookCoverImage = item.BookCoverImage,
                    OrganizationName = item.OrganizationName,
                    AddedToCartAt = item.AddedAt
                })
                .ToList();

            var borrowingPolicy = _borrowTicketService.GetBorrowingPolicy(orgId);
            var dailyLimit = borrowingPolicy?.BorrowLimitPerUser > 0
                ? Math.Min(10, borrowingPolicy.BorrowLimitPerUser)
                : 10;

            var borrowedToday = tickets.Count(t =>
                t.CreatedAt.Date == DateTime.Today &&
                t.Status != TicketStatus.Cancelled &&
                t.Status != TicketStatus.Expired);

            var activeTicketCount = pendingTickets.Count + borrowedTickets.Count;

            var model = new BookCheckoutViewModel
            {
                UserName = userModel.Name,
                DailyBorrowLimit = dailyLimit,
                BorrowedToday = borrowedToday,
                MaxActiveTickets = borrowingPolicy?.MaxActiveTicketsPerUser ?? 0,
                ActiveTicketCount = activeTicketCount,
                BorrowDaysDefault = borrowingPolicy?.BorrowDaysDefault ?? 0,
                CartItems = cartItems,
                PendingTickets = pendingTickets,
                BorrowedTickets = borrowedTickets,
                HistoryTickets = historyTickets
            };

            ViewBag.OrganizationName = this._session.GetString("OrganizationName") ?? "Library";

            return View(model);
        }

        /// <summary>
        /// Add a book to the user's cart.
        /// </summary>
        /// <param name="bookId">Book identifier.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public IActionResult AddToCart(int bookId)
        {
            try
            {
                var userIdString = this._session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Json(new { success = false, message = "Please login to add books to cart." });
                }

                var userModel = _userService.GetUserByUserId(userIdString);
                if (userModel == null)
                {
                    return Json(new { success = false, message = "User account not found." });
                }

                // Add to cart using service
                var success = _bookCheckoutService.AddToCart(userModel.Id, bookId);
                
                if (!success)
                {
                    // Check if book is already in cart
                    if (_bookCheckoutService.IsInCart(userModel.Id, bookId))
                    {
                        return Json(new { success = false, message = "Book is already in your cart." });
                    }
                    
                    // Check if book is available
                    var book = _booksService.GetBookById(bookId);
                    if (book == null)
                    {
                        return Json(new { success = false, message = "Book not found." });
                    }
                    
                    if (book.CopiesAvailable <= 0)
                    {
                        return Json(new { success = false, message = "No copies available for this book." });
                    }
                    
                    return Json(new { success = false, message = "Unable to add book to cart." });
                }

                var cartCount = _bookCheckoutService.GetCartCount(userModel.Id);

                return Json(new
                {
                    success = true,
                    message = "Book added to cart!",
                    cartCount = cartCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Get the count of items in the user's cart.
        /// </summary>
        /// <returns>JSON result with cart count.</returns>
        [HttpGet]
        public IActionResult GetCartCount()
        {
            try
            {
                var userIdString = this._session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Json(new { cartCount = 0 });
                }

                var userModel = _userService.GetUserByUserId(userIdString);
                if (userModel == null)
                {
                    return Json(new { cartCount = 0 });
                }

                var cartCount = _bookCheckoutService.GetCartCount(userModel.Id);
                return Json(new { cartCount });
            }
            catch (Exception ex)
            {
                return Json(new { cartCount = 0 });
            }
        }

        /// <summary>
        /// Remove a book from the user's cart.
        /// </summary>
        /// <param name="bookId">Book identifier.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public IActionResult RemoveFromCart(int bookId)
        {
            try
            {
                var userIdString = this._session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Json(new { success = false, message = "Please login to manage your cart." });
                }

                var userModel = _userService.GetUserByUserId(userIdString);
                if (userModel == null)
                {
                    return Json(new { success = false, message = "User account not found." });
                }

                var success = _bookCheckoutService.RemoveFromCart(userModel.Id, bookId);
                return Json(new { success = success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Toggle wishlist status for a book.
        /// </summary>
        /// <param name="bookId">Book identifier.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public IActionResult ToggleWishlist(int bookId)
        {
            try
            {
                var userIdString = this._session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Json(new { success = false, message = "Please login to manage your wishlist." });
                }

                var userModel = _userService.GetUserByUserId(userIdString);
                if (userModel == null)
                {
                    return Json(new { success = false, message = "User account not found." });
                }

                var success = _wishlistService.ToggleWishlist(userModel.Id, bookId);
                var isInWishlist = _wishlistService.IsInWishlist(userModel.Id, bookId);

                return Json(new { 
                    success = success, 
                    isInWishlist = isInWishlist,
                    message = isInWishlist ? "Added to wishlist!" : "Removed from wishlist!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Check if a book is in the user's wishlist.
        /// </summary>
        /// <param name="bookId">Book identifier.</param>
        /// <returns>JSON result with wishlist status.</returns>
        [HttpGet]
        public IActionResult GetWishlistStatus(int bookId)
        {
            try
            {
                var userIdString = this._session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Json(new { isInWishlist = false });
                }

                var userModel = _userService.GetUserByUserId(userIdString);
                if (userModel == null)
                {
                    return Json(new { isInWishlist = false });
                }

                var isInWishlist = _wishlistService.IsInWishlist(userModel.Id, bookId);
                return Json(new { isInWishlist = isInWishlist });
            }
            catch (Exception ex)
            {
                return Json(new { isInWishlist = false });
            }
        }

        /// <summary>
        /// Create a borrow ticket for the specified book.
        /// </summary>
        /// <param name="bookId">Book identifier.</param>
        /// <returns>JSON result with reference details.</returns>
        [HttpPost]
        public IActionResult BorrowBook(int bookId)
        {
            try
            {
                var userIdString = this._session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Json(new { success = false, message = "Please login to borrow books." });
                }

                var userModel = _userService.GetUserByUserId(userIdString);
                if (userModel == null)
                {
                    return Json(new { success = false, message = "User account not found." });
                }

                var ticket = _borrowTicketService.CreateBorrowRequest(userModel.Id, bookId);

                var orgId = this._session.GetInt32("OrganizationId") ?? 0;
                var policy = _borrowTicketService.GetBorrowingPolicy(orgId);
                var dailyLimit = policy?.BorrowLimitPerUser > 0
                    ? Math.Min(10, policy.BorrowLimitPerUser)
                    : 10;

                var borrowedToday = _borrowTicketService.GetUserTickets(userModel.Id)
                    .Count(t => t.CreatedAt.Date == DateTime.Today &&
                                t.Status != TicketStatus.Cancelled &&
                                t.Status != TicketStatus.Expired);

                return Json(new
                {
                    success = true,
                    ticketId = ticket.Id,
                    referenceCode = ticket.ReferenceCode,
                    bookTitle = ticket.BookTitle,
                    organizationName = ticket.OrganizationName ?? (this._session.GetString("OrganizationName") ?? "your library"),
                    expiresAt = ticket.ExpiresAt.ToString("MMMM d, yyyy h:mm tt"),
                    borrowedToday,
                    dailyLimit
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Confirm borrow for selected cart items and create tickets.
        /// </summary>
        /// <param name="bookIds">List of book IDs to borrow.</param>
        /// <returns>JSON result with reference details.</returns>
        [HttpPost]
        public IActionResult ConfirmBorrow(List<int> bookIds)
        {
            try
            {
                var userIdString = this._session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Json(new { success = false, message = "Please login to borrow books." });
                }

                var userModel = _userService.GetUserByUserId(userIdString);
                if (userModel == null)
                {
                    return Json(new { success = false, message = "User account not found." });
                }

                if (bookIds == null || !bookIds.Any())
                {
                    return Json(new { success = false, message = "No books selected to borrow." });
                }

                var createdTickets = new List<object>();
                var errors = new List<string>();

                foreach (var bookId in bookIds)
                {
                    try
                    {
                        var ticket = _borrowTicketService.CreateBorrowRequest(userModel.Id, bookId);
                        createdTickets.Add(new
                        {
                            ticketId = ticket.Id,
                            referenceCode = ticket.ReferenceCode,
                            bookTitle = ticket.BookTitle,
                            organizationName = ticket.OrganizationName ?? (this._session.GetString("OrganizationName") ?? "your library"),
                            expiresAt = ticket.ExpiresAt.ToString("MMMM d, yyyy h:mm tt")
                        });
                    }
                    catch (Exception ex)
                    {
                        // Get book title for better error message
                        var book = _booksService.GetBookById(bookId);
                        var bookTitle = book?.Title ?? $"Book ID {bookId}";
                        errors.Add($"Failed to borrow '{bookTitle}': {ex.Message}");
                    }
                }

                // Remove successfully borrowed books from cart
                if (createdTickets.Any())
                {
                    foreach (var bookId in bookIds)
                    {
                        _bookCheckoutService.RemoveFromCart(userModel.Id, bookId);
                    }
                }

                if (createdTickets.Any())
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully created {createdTickets.Count} borrow request(s).",
                        tickets = createdTickets,
                        errors = errors
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create any borrow requests.", errors = errors });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a borrow ticket created by the current user.
        /// </summary>
        /// <param name="ticketId">Ticket identifier.</param>
        /// <returns>JSON indicating success or failure.</returns>
        [HttpPost]
        public IActionResult CancelBorrowTicket(int ticketId)
        {
            try
            {
                var userIdString = this._session.GetString("UserId");
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Json(new { success = false, message = "Please login to manage your tickets." });
                }

                var userModel = _userService.GetUserByUserId(userIdString);
                if (userModel == null)
                {
                    return Json(new { success = false, message = "User account not found." });
                }

                var ticket = _borrowTicketService.GetTicketById(ticketId);
                if (ticket == null || ticket.UserId != userModel.Id)
                {
                    return Json(new { success = false, message = "Borrow ticket not found." });
                }

                if (ticket.Status != TicketStatus.NotYetClaimed)
                {
                    return Json(new { success = false, message = "Only unclaimed tickets can be cancelled." });
                }

                _borrowTicketService.CancelTicket(ticketId, "Cancelled by borrower");

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a ticket (alias for CancelBorrowTicket for consistency)
        /// </summary>
        /// <param name="ticketId">Ticket identifier.</param>
        /// <returns>JSON indicating success or failure.</returns>
        [HttpPost]
        public IActionResult CancelTicket(int ticketId)
        {
            return CancelBorrowTicket(ticketId);
        }

        /// <summary>
        /// User Settings Action - GET
        /// </summary>
        /// <param name="tab">Selected tab (borrowed-books, unreturned-books, history, wishlist, account-settings)</param>
        /// <returns> UserSettings View </returns>
        [HttpGet]
        public IActionResult UserSettings(string tab = "borrowed-books")
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

            // Set selected tab
            ViewBag.SelectedTab = tab;

            // Load data based on selected tab
            var viewModel = new UserSettingsViewModel
            {
                Id = userModel.Id,
                UserId = userModel.UserId,
                Name = userModel.Name,
                OrganizationId = userModel.OrganizationId,
                ProfilePicture = userModel.ProfilePicture,
                CreatedTime = userModel.CreatedTime,
                UpdatedTime = userModel.UpdatedTime,
                CreatedBy = userModel.CreatedBy,
                UpdatedBy = userModel.UpdatedBy
            };

            // Load tab-specific data
            switch (tab)
            {
                case "borrowed-books":
                    viewModel.BorrowedBooks = _borrowTicketService.GetUserBorrowedBooks(userModel.Id);
                    break;
                case "unreturned-books":
                    viewModel.OverdueBooks = _borrowTicketService.GetUserOverdueBooks(userModel.Id);
                    break;
                case "history":
                    viewModel.HistoryBooks = _borrowTicketService.GetUserBorrowHistory(userModel.Id);
                    break;
                case "pending-reservations":
                    var allTickets = _borrowTicketService.GetUserTickets(userModel.Id);
                    viewModel.PendingTickets = allTickets?.Where(t => t.Status == TicketStatus.NotYetClaimed).ToList() ?? new List<BorrowTicketViewModel>();
                    break;
                case "wishlist":
                    viewModel.WishlistItems = _wishlistService.GetUserWishlist(userModel.Id);
                    break;
            }

            return View(viewModel);
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

        /// <summary>
        /// Book Details Action
        /// </summary>
        /// <param name="id">Book ID</param>
        /// <returns> BookDetails View </returns>
        public IActionResult BookDetails(int id)
        {
            // Check if user is logged in
            var userId = this._session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("LoginUser", "Account");
            }

            var userModel = _userService.GetUserByUserId(userId);
            if (userModel == null)
            {
                return RedirectToAction("LoginUser", "Account");
            }

            // Get book details with user rating
            var book = _booksService.GetBookDetails(id, userModel.Id);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Book not found.";
                return RedirectToAction("Index");
            }

            // Get books by same author for "More by Author" section
            var orgId = this._session.GetInt32("OrganizationId") ?? 0;
            var booksByAuthor = _booksService.GetBookCatalog(orgId)
                .Where(b => b.Author == book.Author && b.Id != book.Id)
                .Take(5)
                .ToList();
            
            ViewBag.BooksByAuthor = booksByAuthor;

            // Get reviews for display
            var reviews = _reviewService.GetBookReviewsDisplay(id, userModel.Id);
            ViewBag.Reviews = reviews;
            
            // Pass current user's database ID to view for comparison
            ViewBag.CurrentUserId = userModel.Id;

            return View(book);
        }

        /// <summary>
        /// Rate Book Action
        /// </summary>
        /// <param name="bookId">Book ID</param>
        /// <param name="rating">Rating value (1-5 or null)</param>
        /// <param name="reviewText">Review text (max 250 characters)</param>
        /// <returns> JSON result </returns>
        [HttpPost]
        public IActionResult RateBook(int bookId, int? rating, string reviewText = null)
        {
            try
            {
                var userId = this._session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Please login to rate books." });
                }

                var userModel = _userService.GetUserByUserId(userId);
                if (userModel == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Validate rating
                if (rating.HasValue && (rating < 1 || rating > 5))
                {
                    return Json(new { success = false, message = "Rating must be between 1 and 5." });
                }

                // Validate review text length
                if (!string.IsNullOrEmpty(reviewText) && reviewText.Length > 250)
                {
                    return Json(new { success = false, message = "Review text must not exceed 250 characters." });
                }

                // Save rating and review
                _reviewService.SaveRating(userModel.Id, bookId, rating, reviewText);

                // Get updated average rating
                var averageRating = _reviewService.GetBookAverageRating(bookId);

                return Json(new { 
                    success = true, 
                    message = "Review saved successfully!",
                    averageRating = averageRating,
                    userRating = rating
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error saving review: {ex.Message}" });
            }
        }

        /// <summary>
        /// Like Review Action
        /// </summary>
        /// <param name="reviewId">Review ID</param>
        /// <returns> JSON result </returns>
        [HttpPost]
        public IActionResult LikeReview(int reviewId)
        {
            try
            {
                var userId = this._session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "Please login to like reviews." });
                }

                var userModel = _userService.GetUserByUserId(userId);
                if (userModel == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Check if user is trying to like their own review
                if (_reviewService.IsReviewByUser(userModel.Id, reviewId))
                {
                    return Json(new { success = false, message = "You cannot like your own review." });
                }

                // Toggle like
                var result = _reviewService.ToggleLike(userModel.Id, reviewId);

                return Json(new { 
                    success = true, 
                    isLiked = result.isLiked,
                    likeCount = result.likeCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
