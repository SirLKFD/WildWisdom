using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IOrgBooksRepository _booksRepository;
        private readonly IBookReviewService _bookReviewService;
        private readonly IMapper _mapper;

        public WishlistService(
            IWishlistRepository wishlistRepository,
            IOrgBooksRepository booksRepository,
            IBookReviewService bookReviewService,
            IMapper mapper)
        {
            _wishlistRepository = wishlistRepository;
            _booksRepository = booksRepository;
            _bookReviewService = bookReviewService;
            _mapper = mapper;
        }

        public List<WishlistItemViewModel> GetUserWishlist(int userId)
        {
            var wishlistItems = _wishlistRepository.GetUserWishlist(userId);
            var viewModels = new List<WishlistItemViewModel>();

            foreach (var item in wishlistItems)
            {
                if (item.OrgBook != null)
                {
                    // Get book review statistics
                    var reviews = _bookReviewService.GetBookReviews(item.OrgBook.Id);
                    var averageRating = reviews.Any() ? (double)reviews.Average(r => r.Rating) : 0.0;
                    var reviewCount = reviews.Count;

                    viewModels.Add(new WishlistItemViewModel
                    {
                        BookId = item.OrgBook.Id,
                        BookTitle = item.OrgBook.Title,
                        BookAuthor = item.OrgBook.Author,
                        BookCoverImage = item.OrgBook.CoverImage,
                        OrganizationName = item.OrgBook.Organization?.Name ?? "Library",
                        AddedAt = item.AddedAt,
                        CopiesAvailable = item.OrgBook.CopiesAvailable,
                        AverageRating = Math.Round(averageRating, 1),
                        ReviewCount = reviewCount
                    });
                }
            }

            return viewModels;
        }

        public bool AddToWishlist(int userId, int bookId)
        {
            try
            {
                // Check if already in wishlist
                if (_wishlistRepository.IsInWishlist(userId, bookId))
                {
                    return false; // Already in wishlist
                }

                // Get book details
                var book = _booksRepository.GetById(bookId);
                if (book == null)
                {
                    return false; // Book not found
                }

                // Add to wishlist
                var wishlistItem = new Wishlist
                {
                    UserId = userId,
                    OrgBookId = bookId,
                    AddedAt = DateTime.Now,
                    CreatedBy = "System",
                    CreatedTime = DateTime.Now,
                    UpdatedBy = "System",
                    UpdatedTime = DateTime.Now
                };

                _wishlistRepository.AddToWishlist(wishlistItem);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveFromWishlist(int userId, int bookId)
        {
            try
            {
                _wishlistRepository.RemoveFromWishlist(userId, bookId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ToggleWishlist(int userId, int bookId)
        {
            if (_wishlistRepository.IsInWishlist(userId, bookId))
            {
                return RemoveFromWishlist(userId, bookId);
            }
            else
            {
                return AddToWishlist(userId, bookId);
            }
        }

        public bool IsInWishlist(int userId, int bookId)
        {
            return _wishlistRepository.IsInWishlist(userId, bookId);
        }
    }
}
