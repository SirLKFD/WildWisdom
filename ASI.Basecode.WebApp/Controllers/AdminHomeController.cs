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
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Admin Home Controller
    /// </summary>
    public class AdminHomeController : ControllerBase<AdminHomeController>
    {
        private readonly IOrgBooksService _booksService;
        private readonly IAdminService _adminService;
        private readonly IBorrowTicketService _borrowTicketService;
        private readonly IOrganizationService _organizationService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="booksService"></param>
        /// <param name="adminService"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="mapper"></param>
        public AdminHomeController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IOrgBooksService booksService,
            IAdminService adminService,
            IBorrowTicketService borrowTicketService,
            IOrganizationService organizationService,
            IWebHostEnvironment webHostEnvironment,
            IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _booksService = booksService;
            _adminService = adminService;
            _borrowTicketService = borrowTicketService;
            _organizationService = organizationService;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Check if current user is an admin
        /// </summary>
        private bool IsAdminUser()
        {
            var userRole = this._session.GetInt32("UserRole");
            return userRole != null && userRole == 1;
        }

        /// <summary>
        /// Verify admin authorization and redirect if not authorized
        /// </summary>
        private IActionResult VerifyAdminAccess()
        {
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("LoginAdmin", "Account");
            }

            var userRole = this._session.GetInt32("UserRole");
            if (userRole == 0) // Explicitly check if regular User
            {
                TempData["ErrorMessage"] = "Access denied. You are logged in as a regular User.";
                return RedirectToAction("Index", "UserHome");
            }

            // If userRole is 1 or not set, allow access (default to Admin for this controller)
            return null;
        }

        /// <summary>
        /// Dashboard Page
        /// </summary>
        /// <returns> Dashboard View </returns>
        public IActionResult Dashboard()
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var orgId = this._session.GetInt32("OrganizationId") ?? 0;

                // Get all books for the organization
                var allBooks = _booksService.GetAllBooksByOrganization(orgId);
                var totalBooks = allBooks.Count(b => b.IsActive);

                // Get all tickets for the organization
                var allTickets = _borrowTicketService.GetOrganizationTickets(orgId);

                // Calculate statistics
                var borrowedToday = allTickets.Count(t => 
                    t.CreatedAt.Date == DateTime.Today && 
                    t.Status != TicketStatus.Cancelled && 
                    t.Status != TicketStatus.Expired);

                var pendingClaims = allTickets.Count(t => t.Status == TicketStatus.NotYetClaimed);
                var activeBorrows = allTickets.Count(t => t.Status == TicketStatus.NotYetReturned);
                var overdueCount = allTickets.Count(t => 
                    t.Status == TicketStatus.NotYetReturned && 
                    t.DueAt.HasValue && 
                    t.DueAt.Value < DateTime.Now);

                // Get recent activities (last 5)
                var recentActivities = allTickets
                    .OrderByDescending(t => t.ReturnedAt ?? t.CheckedOutAt ?? t.ClaimedAt ?? t.CreatedAt)
                    .Take(5)
                    .ToList();

                // Get recent books (last 5)
                var recentBooks = allBooks
                    .Where(b => b.IsActive)
                    .OrderByDescending(b => b.CreatedTime)
                    .Take(5)
                    .ToList();

                var model = new DashboardViewModel
                {
                    TotalBooks = totalBooks,
                    BorrowedToday = borrowedToday,
                    PendingClaims = pendingClaims,
                    ActiveBorrows = activeBorrows,
                    OverdueCount = overdueCount,
                    RecentActivities = recentActivities,
                    RecentBooks = recentBooks
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Return empty model on error
                var model = new DashboardViewModel
                {
                    TotalBooks = 0,
                    BorrowedToday = 0,
                    PendingClaims = 0,
                    ActiveBorrows = 0,
                    OverdueCount = 0
                };
                return View(model);
            }
        }

        /// <summary>
        /// Returns Admin Home View with Books List.
        /// </summary>
        /// <param name="search">Search term</param>
        /// <param name="category">Category filter</param>
        /// <param name="genre">Genre filter</param>
        /// <param name="status">Status filter</param>
        /// <param name="availability">Availability filter</param>
        /// <param name="sortBy">Sort order</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns> Admin Home View </returns>
        public IActionResult Index(string search = "", string category = "", string genre = "", string status = "", string availability = "", string sortBy = "title", int page = 1, int pageSize = 25)
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            // Get all books (including inactive) from the admin's organization
            var orgId = this._session.GetInt32("OrganizationId") ?? 0;
            var allBooks = _booksService.GetAllBooksByOrganization(orgId);

            // Apply filters
            var filteredBooks = allBooks.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                filteredBooks = filteredBooks.Where(b => 
                    b.Title.ToLower().Contains(searchLower) || 
                    b.Author.ToLower().Contains(searchLower) ||
                    b.ISBN.ToLower().Contains(searchLower) ||
                    (b.Publisher != null && b.Publisher.ToLower().Contains(searchLower)));
            }

            // Category filter
            if (!string.IsNullOrEmpty(category))
            {
                filteredBooks = filteredBooks.Where(b => b.Category == category);
            }

            // Genre filter
            if (!string.IsNullOrEmpty(genre))
            {
                filteredBooks = filteredBooks.Where(b => b.Genre == genre);
            }

            // Status filter
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "active")
                {
                    filteredBooks = filteredBooks.Where(b => b.IsActive);
                }
                else if (status == "inactive")
                {
                    filteredBooks = filteredBooks.Where(b => !b.IsActive);
                }
            }

            // Availability filter
            if (!string.IsNullOrEmpty(availability))
            {
                if (availability == "available")
                {
                    filteredBooks = filteredBooks.Where(b => b.CopiesAvailable > 0);
                }
                else if (availability == "out-of-stock")
                {
                    filteredBooks = filteredBooks.Where(b => b.CopiesAvailable == 0);
                }
            }

            // Apply sorting
            switch (sortBy)
            {
                case "title":
                    filteredBooks = filteredBooks.OrderBy(b => b.Title);
                    break;
                case "author":
                    filteredBooks = filteredBooks.OrderBy(b => b.Author);
                    break;
                case "date-added":
                    filteredBooks = filteredBooks.OrderByDescending(b => b.CreatedTime);
                    break;
                case "copies-available":
                    filteredBooks = filteredBooks.OrderByDescending(b => b.CopiesAvailable);
                    break;
                default:
                    filteredBooks = filteredBooks.OrderBy(b => b.Title);
                    break;
            }

            // Get total count for pagination
            var totalBooks = filteredBooks.Count();
            var totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);

            // Apply pagination
            var paginatedBooks = filteredBooks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get distinct categories and genres for filters
            var categories = allBooks.Where(b => !string.IsNullOrEmpty(b.Category))
                                  .Select(b => b.Category)
                                  .Distinct()
                                  .OrderBy(c => c)
                                  .ToList();
            
            var genres = allBooks.Where(b => !string.IsNullOrEmpty(b.Genre))
                             .Select(b => b.Genre)
                             .Distinct()
                             .OrderBy(g => g)
                             .ToList();

            // Set ViewBag properties
            ViewBag.SearchTerm = search;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedGenre = genre;
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedAvailability = availability;
            ViewBag.SortBy = sortBy;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalBooks = totalBooks;
            ViewBag.TotalPages = totalPages;
            ViewBag.Categories = categories;
            ViewBag.Genres = genres;

            return View(paginatedBooks);
        }

        /// <summary>
        /// Reference lookup page for borrow tickets.
        /// </summary>
        [HttpGet]
        public IActionResult ReferenceLookup()
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            // Get organization information for auto-filling reference number
            var orgId = this._session.GetInt32("OrganizationId") ?? 0;
            var organization = _organizationService.GetOrganizationById(orgId);
            var organizationCode = $"ORG{orgId:D3}";

            var viewModel = new BorrowTicketLookupViewModel
            {
                OrganizationCode = organizationCode
            };

            return View(viewModel);
        }

        /// <summary>
        /// Get pending claims for the admin's organization.
        /// </summary>
        /// <returns>JSON payload with pending claims list.</returns>
        [HttpGet]
        public IActionResult GetPendingClaims()
        {
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username) || !IsAdminUser())
            {
                return Json(new { success = false, message = "Unauthorized. Please login as an administrator." });
            }

            try
            {
                var orgId = this._session.GetInt32("OrganizationId") ?? 0;
                var pendingClaims = _borrowTicketService.GetPendingClaims(orgId);

                var claims = pendingClaims.Select(claim => new
                {
                    referenceCode = claim.ReferenceCode,
                    bookTitle = claim.BookTitle,
                    userName = claim.UserName,
                    expiresAt = claim.ExpiresAt.ToString("MMMM d, yyyy h:mm tt"),
                    createdAt = claim.CreatedAt.ToString("MMMM d, yyyy h:mm tt")
                }).ToList();

                return Json(new { success = true, claims = claims });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Confirm a claim for a borrow ticket.
        /// </summary>
        /// <param name="referenceCode">Reference number of the ticket.</param>
        /// <param name="notes">Optional notes for the confirmation.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public IActionResult ConfirmClaim(string referenceCode, string notes = "")
        {
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username) || !IsAdminUser())
            {
                return Json(new { success = false, message = "Unauthorized. Please login as an administrator." });
            }

            try
            {
                var orgId = this._session.GetInt32("OrganizationId") ?? 0;
                var adminId = this._session.GetInt32("UserId") ?? 0;

                // Verify the ticket belongs to this organization
                var ticket = _borrowTicketService.GetTicketByReferenceCode(referenceCode);
                if (ticket == null || ticket.OrganizationId != orgId)
                {
                    return Json(new { success = false, message = "Ticket not found for your organization." });
                }

                _borrowTicketService.ClaimTicket(referenceCode, adminId, notes ?? "Confirmed by admin");

                return Json(new { success = true, message = "Claim confirmed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Check whether a borrow ticket reference exists within the admin's organization.
        /// </summary>
        /// <param name="referenceCode">Reference number supplied by the patron.</param>
        /// <returns>JSON payload describing the ticket, if found.</returns>
        [HttpPost]
        public IActionResult CheckReference(string referenceCode)
        {
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username) || !IsAdminUser())
            {
                return Json(new { success = false, message = "Unauthorized. Please login as an administrator." });
            }

            if (string.IsNullOrWhiteSpace(referenceCode))
            {
                return Json(new { success = false, message = "Please enter a reference number to search." });
            }

            var orgId = this._session.GetInt32("OrganizationId") ?? 0;
            var ticket = _borrowTicketService.GetTicketByReferenceCode(referenceCode.Trim());
            if (ticket == null || ticket.OrganizationId != orgId)
            {
                return Json(new { success = false, message = "Reference number not found for your organization." });
            }

            return Json(new
            {
                success = true,
                ticket = new
                {
                    ticket.ReferenceCode,
                    ticket.BookTitle,
                    ticket.BookAuthor,
                    ticket.UserName,
                    ticket.OrganizationName,
                    status = ticket.StatusDisplay,
                    createdAt = ticket.CreatedAt.ToString("MMMM d, yyyy h:mm tt"),
                    expiresAt = ticket.ExpiresAt.ToString("MMMM d, yyyy h:mm tt"),
                    claimedAt = ticket.ClaimedAt?.ToString("MMMM d, yyyy h:mm tt"),
                    checkedOutAt = ticket.CheckedOutAt?.ToString("MMMM d, yyyy h:mm tt"),
                    dueAt = ticket.DueAt?.ToString("MMMM d, yyyy h:mm tt"),
                    returnedAt = ticket.ReturnedAt?.ToString("MMMM d, yyyy h:mm tt"),
                    notes = ticket.Notes
                }
            });
        }

        /// <summary>
        /// Show Add Book Form
        /// </summary>
        [HttpGet]
        public IActionResult AddBook()
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            return View(new OrgBooksViewModel());
        }

        /// <summary>
        /// Add New Book
        /// </summary>
        [HttpPost]
        public IActionResult AddBook(OrgBooksViewModel model, IFormFile CoverImageFile)
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields.";
                    return View(model);
                }

                // Handle file upload
                if (CoverImageFile != null && CoverImageFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(CoverImageFile.FileName).ToLower();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["ErrorMessage"] = "Only JPEG, PNG, and GIF images are allowed.";
                        return View(model);
                    }

                    // Validate file size (5MB max)
                    if (CoverImageFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "File size must be less than 5MB.";
                        return View(model);
                    }

                    // Create upload directory if it doesn't exist
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "books");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        CoverImageFile.CopyTo(fileStream);
                    }

                    // Store relative path in model
                    model.CoverImage = "/uploads/books/" + uniqueFileName;
                }

                // Get admin's organization ID from session
                var adminUsername = this._session.GetString("UserName");
                model.OrganizationId = this._session.GetInt32("OrganizationId") ?? 0;

                model.CreatedBy = adminUsername;
                model.UpdatedBy = adminUsername;
                model.CreatedTime = DateTime.Now;
                model.UpdatedTime = DateTime.Now;
                model.CopiesAvailable = model.CopiesTotal;
                model.IsActive = true;

                _booksService.AddBook(model);

                TempData["SuccessMessage"] = "Book added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error adding book: {ex.Message}";
                return View(model);
            }
        }

        /// <summary>
        /// Show Edit Book Form
        /// </summary>
        [HttpGet]
        public IActionResult EditBook(int id)
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var book = _booksService.GetBookById(id);
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Book not found.";
                    return RedirectToAction("Index");
                }

                return View(book);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading book: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Update Book
        /// </summary>
        [HttpPost]
        public IActionResult EditBook(OrgBooksViewModel model, IFormFile CoverImageFile)
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please fill in all required fields.";
                    return View(model);
                }

                // Handle file upload (only if new file is provided)
                if (CoverImageFile != null && CoverImageFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(CoverImageFile.FileName).ToLower();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["ErrorMessage"] = "Only JPEG, PNG, and GIF images are allowed.";
                        return View(model);
                    }

                    // Validate file size (5MB max)
                    if (CoverImageFile.Length > 5 * 1024 * 1024)
                    {
                        TempData["ErrorMessage"] = "File size must be less than 5MB.";
                        return View(model);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(model.CoverImage))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, model.CoverImage.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Create upload directory if it doesn't exist
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "books");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var uniqueFileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        CoverImageFile.CopyTo(fileStream);
                    }

                    // Update model with new path
                    model.CoverImage = "/uploads/books/" + uniqueFileName;
                }

                var adminUsername = this._session.GetString("UserName");
                model.UpdatedBy = adminUsername;
                model.UpdatedTime = DateTime.Now;

                _booksService.UpdateBook(model);

                TempData["SuccessMessage"] = "Book updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating book: {ex.Message}";
                return View(model);
            }
        }

        /// <summary>
        /// Delete Book
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult DeleteBook(int id)
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                _booksService.DeleteBook(id);
                TempData["SuccessMessage"] = "Book deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting book: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Toggle Book Active Status
        /// </summary>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult ToggleBookStatus(int id)
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var book = _booksService.GetBookById(id);
                if (book != null)
                {
                    book.IsActive = !book.IsActive;
                    book.UpdatedBy = this._session.GetString("UserName");
                    book.UpdatedTime = DateTime.Now;
                    _booksService.UpdateBook(book);

                    TempData["SuccessMessage"] = $"Book {(book.IsActive ? "activated" : "deactivated")} successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error toggling book status: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Admin Settings Page
        /// </summary>
        [HttpGet]
        public IActionResult AdminSettings()
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var adminId = this._session.GetInt32("UserId") ?? 0;
                var model = _adminService.GetAdminSettings(adminId);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading settings: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Update Admin Settings
        /// </summary>
        [HttpPost]
        public IActionResult UpdateAdminSettings(AdminSettingsViewModel model, IFormFile profilePictureFile)
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                // Validate email uniqueness if email is being changed
                var adminId = this._session.GetInt32("UserId") ?? 0;
                var currentAdmin = _adminService.GetAdminSettings(adminId);
                if (currentAdmin != null && model.Email != currentAdmin.Email)
                {
                    // Check if email already exists for another admin
                    if (_adminService.AdminExists(model.Email))
                    {
                        ModelState.AddModelError("Email", "This email address is already registered by another admin.");
                        return View("AdminSettings", model);
                    }
                }

                // Handle profile picture upload
                if (profilePictureFile != null && profilePictureFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePictureFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        profilePictureFile.CopyTo(fileStream);
                    }

                    model.ProfilePicture = "/uploads/profiles/" + uniqueFileName;
                    this._session.SetString("ProfilePicture", model.ProfilePicture);
                }

                _adminService.UpdateAdminSettings(model);

                // Update session data
                this._session.SetString("UserName", model.AdminUsername);

                TempData["SuccessMessage"] = "Settings updated successfully!";
                return RedirectToAction("AdminSettings");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating settings: {ex.Message}";
                return View("AdminSettings", model);
            }
        }

        /// <summary>
        /// Delete Admin Account
        /// </summary>
        [HttpPost]
        public IActionResult DeleteAdminAccount()
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            try
            {
                var adminId = this._session.GetInt32("UserId") ?? 0;

                if (!_adminService.CanDeleteAccount(adminId))
                {
                    TempData["ErrorMessage"] = "Cannot delete account with pending transactions.";
                    return RedirectToAction("AdminSettings");
                }

                _adminService.DeleteAdminAccount(adminId);

                // Clear session
                HttpContext.Session.Clear();

                TempData["SuccessMessage"] = "Account deleted successfully.";
                return RedirectToAction("LoginAdmin", "Account");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting account: {ex.Message}";
                return RedirectToAction("AdminSettings");
            }
        }

        /// <summary>
        /// Get unreturned books for the admin's organization.
        /// </summary>
        /// <returns>JSON payload with unreturned books list.</returns>
        [HttpGet]
        public IActionResult GetUnreturnedBooks()
        {
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username) || !IsAdminUser())
            {
                return Json(new { success = false, message = "Unauthorized. Please login as an administrator." });
            }

            try
            {
                var orgId = this._session.GetInt32("OrganizationId") ?? 0;
                var unreturnedBooks = _borrowTicketService.GetOrganizationTickets(orgId, TicketStatus.NotYetReturned);
                
                var result = unreturnedBooks.Select(ticket => new
                {
                    id = ticket.Id,
                    referenceCode = ticket.ReferenceCode,
                    bookTitle = ticket.BookTitle,
                    bookAuthor = ticket.BookAuthor,
                    userName = ticket.UserName,
                    checkedOutAt = ticket.CheckedOutAt?.ToString("MMM dd, yyyy"),
                    dueAt = ticket.DueAt?.ToString("MMM dd, yyyy"),
                    isOverdue = ticket.IsOverdue,
                    daysOverdue = ticket.IsOverdue ? (int)(DateTime.Now - ticket.DueAt.Value).TotalDays : 0
                }).ToList();

                return Json(new { success = true, books = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Confirm a book return.
        /// </summary>
        /// <param name="referenceCode">Reference number of the ticket.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public IActionResult ConfirmReturn(string referenceCode)
        {
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username) || !IsAdminUser())
            {
                return Json(new { success = false, message = "Unauthorized. Please login as an administrator." });
            }

            try
            {
                var orgId = this._session.GetInt32("OrganizationId") ?? 0;
                var adminId = this._session.GetInt32("UserId") ?? 0;

                // Verify the ticket belongs to this organization
                var ticket = _borrowTicketService.GetTicketByReferenceCode(referenceCode);
                if (ticket == null || ticket.OrganizationId != orgId)
                {
                    return Json(new { success = false, message = "Ticket not found for your organization." });
                }

                _borrowTicketService.ReturnBook(referenceCode, adminId);
                return Json(new { success = true, message = "Book return confirmed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Return History Page
        /// </summary>
        /// <returns>History View</returns>
        [HttpGet]
        public IActionResult History()
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            return View();
        }

        /// <summary>
        /// Get return history with filtering and pagination
        /// </summary>
        /// <param name="searchQuery">Search by book title, user name, or reference code</param>
        /// <param name="startDate">Start date filter</param>
        /// <param name="endDate">End date filter</param>
        /// <param name="statusFilter">Status filter (all, returned, cancelled, expired, lost)</param>
        /// <param name="fineStatusFilter">Fine status filter (all, has-fine, no-fine, fine-paid, fine-unpaid)</param>
        /// <param name="sortBy">Sort order (latest, oldest, highest-fine, lowest-fine)</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Records per page</param>
        /// <returns>JSON with return history data</returns>
        [HttpGet]
        public IActionResult GetReturnHistory(string searchQuery, DateTime? startDate, DateTime? endDate, 
            string statusFilter = "all", string fineStatusFilter = "all", string sortBy = "latest", 
            int page = 1, int pageSize = 20)
        {
            var username = this._session.GetString("UserName");
            if (string.IsNullOrEmpty(username) || !IsAdminUser())
            {
                return Json(new { success = false, message = "Unauthorized. Please login as an administrator." });
            }

            try
            {
                var orgId = this._session.GetInt32("OrganizationId") ?? 0;
                
                // Get all tickets for the organization (not just returned)
                var allTickets = _borrowTicketService.GetOrganizationTickets(orgId);
                
                // Apply status filter
                if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all")
                {
                    TicketStatus? targetStatus = statusFilter.ToLower() switch
                    {
                        "returned" => TicketStatus.Returned,
                        "cancelled" => TicketStatus.Cancelled,
                        "expired" => TicketStatus.Expired,
                        "lost" => TicketStatus.Lost,
                        _ => null
                    };
                    
                    if (targetStatus.HasValue)
                    {
                        allTickets = allTickets.Where(t => t.Status == targetStatus.Value).ToList();
                    }
                }
                
                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    var query = searchQuery.ToLower();
                    allTickets = allTickets.Where(t => 
                        t.BookTitle.ToLower().Contains(query) ||
                        t.UserName.ToLower().Contains(query) ||
                        t.ReferenceCode.ToLower().Contains(query)
                    ).ToList();
                }

                // Apply date range filter
                if (startDate.HasValue)
                {
                    allTickets = allTickets.Where(t => t.ReturnedAt >= startDate.Value).ToList();
                }
                if (endDate.HasValue)
                {
                    allTickets = allTickets.Where(t => t.ReturnedAt <= endDate.Value.AddDays(1).AddTicks(-1)).ToList();
                }
                
                // Apply fine status filter
                if (!string.IsNullOrWhiteSpace(fineStatusFilter) && fineStatusFilter != "all")
                {
                    allTickets = fineStatusFilter.ToLower() switch
                    {
                        "has-fine" => allTickets.Where(t => t.AccruedFine > 0).ToList(),
                        "no-fine" => allTickets.Where(t => t.AccruedFine == 0).ToList(),
                        "fine-paid" => allTickets.Where(t => t.AccruedFine > 0 && t.IsFinePaid).ToList(),
                        "fine-unpaid" => allTickets.Where(t => t.AccruedFine > 0 && !t.IsFinePaid).ToList(),
                        _ => allTickets
                    };
                }
                
                // Apply sorting
                allTickets = sortBy.ToLower() switch
                {
                    "oldest" => allTickets.OrderBy(t => t.ReturnedAt ?? t.CreatedAt).ToList(),
                    "highest-fine" => allTickets.OrderByDescending(t => t.AccruedFine).ToList(),
                    "lowest-fine" => allTickets.OrderBy(t => t.AccruedFine).ToList(),
                    _ => allTickets.OrderByDescending(t => t.ReturnedAt ?? t.CreatedAt).ToList() // default: latest
                };

                // Calculate pagination
                var totalCount = allTickets.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var skip = (page - 1) * pageSize;
                var pagedTickets = allTickets.Skip(skip).Take(pageSize).ToList();

                // Format the data for display
                var result = pagedTickets.Select(ticket => new
                {
                    id = ticket.Id,
                    referenceCode = ticket.ReferenceCode,
                    bookTitle = ticket.BookTitle,
                    bookAuthor = ticket.BookAuthor,
                    userName = ticket.UserName,
                    checkedOutAt = ticket.CheckedOutAt?.ToString("MMM dd, yyyy"),
                    dueAt = ticket.DueAt?.ToString("MMM dd, yyyy"),
                    returnedAt = ticket.ReturnedAt?.ToString("MMM dd, yyyy h:mm tt"),
                    duration = ticket.CheckedOutAt.HasValue && ticket.ReturnedAt.HasValue 
                        ? (int)(ticket.ReturnedAt.Value - ticket.CheckedOutAt.Value).TotalDays 
                        : 0,
                    fineAmount = ticket.AccruedFine,
                    isFinePaid = ticket.IsFinePaid,
                    returnedToEmployeeId = ticket.ReturnedToEmployeeId,
                    notes = ticket.Notes,
                    status = ticket.StatusDisplay
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = result,
                    pagination = new
                    {
                        currentPage = page,
                        totalPages = totalPages,
                        totalCount = totalCount,
                        pageSize = pageSize,
                        hasNext = page < totalPages,
                        hasPrevious = page > 1
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
