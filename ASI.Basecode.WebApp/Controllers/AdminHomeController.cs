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
    /// Admin Home Controller
    /// </summary>
    public class AdminHomeController : ControllerBase<AdminHomeController>
    {
        private readonly IOrgBooksService _booksService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="configuration"></param>
        /// <param name="booksService"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="mapper"></param>
        public AdminHomeController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IOrgBooksService booksService,
            IWebHostEnvironment webHostEnvironment,
            IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _booksService = booksService;
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
        /// Returns Admin Home View with Books List.
        /// </summary>
        /// <returns> Admin Home View </returns>
        public IActionResult Index()
        {
            var accessCheck = VerifyAdminAccess();
            if (accessCheck != null) return accessCheck;

            // Get all books (including inactive) from the admin's organization
            var orgId = this._session.GetInt32("OrganizationId") ?? 0;
            var books = _booksService.GetAllBooksByOrganization(orgId);
            return View(books);
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
    }
}

