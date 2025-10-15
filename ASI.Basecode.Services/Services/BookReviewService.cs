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
    public class BookReviewService : IBookReviewService
    {
        private readonly IBookReviewRepository _repository;
        private readonly IMapper _mapper;

        public BookReviewService(IBookReviewRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// Get or create user review for a book
        /// </summary>
        public BookReviewViewModel GetOrCreateUserReview(int userId, int bookId)
        {
            var review = _repository.GetReviewByUserAndBook(userId, bookId);
            
            if (review == null)
            {
                review = new BookReview
                {
                    UserId = userId,
                    OrgBookId = bookId,
                    Rating = null,
                    CreatedTime = DateTime.Now,
                    UpdatedTime = DateTime.Now
                };
            }

            return _mapper.Map<BookReviewViewModel>(review);
        }

        /// <summary>
        /// Save user rating and review for a book
        /// </summary>
        public void SaveRating(int userId, int bookId, int? rating, string reviewText = null)
        {
            var review = new BookReview
            {
                UserId = userId,
                OrgBookId = bookId,
                Rating = rating,
                ReviewText = reviewText
            };

            _repository.AddOrUpdateReview(review);
        }

        /// <summary>
        /// Get average rating for a book
        /// </summary>
        public decimal? GetBookAverageRating(int bookId)
        {
            return _repository.GetBookAverageRating(bookId);
        }

        /// <summary>
        /// Get user's rating for a book
        /// </summary>
        public int? GetUserRating(int userId, int bookId)
        {
            var review = _repository.GetReviewByUserAndBook(userId, bookId);
            return review?.Rating;
        }

        /// <summary>
        /// Get all reviews for a book (for calculating statistics)
        /// </summary>
        public List<BookReviewViewModel> GetBookReviews(int bookId)
        {
            var reviews = _repository.GetBookReviews(bookId)
                .Where(r => r.Rating.HasValue) // Only include reviews with ratings
                .ToList();

            return _mapper.Map<List<BookReviewViewModel>>(reviews);
        }

        /// <summary>
        /// Get book reviews for display with user info and like counts
        /// Excludes the current user's review (shown separately in "Your Review" section)
        /// </summary>
        public List<BookReviewDisplayViewModel> GetBookReviewsDisplay(int bookId, int currentUserId)
        {
            var reviewsWithUserInfo = _repository.GetReviewsWithUserInfo(bookId);

            var displayList = reviewsWithUserInfo
                .Where(r => r.Review.UserId != currentUserId) // Exclude current user's review
                .Select(r => new BookReviewDisplayViewModel
                {
                    ReviewId = r.Review.Id,
                    UserId = r.Review.UserId,
                    UserName = r.UserName,
                    UserProfilePicture = r.UserProfilePicture,
                    Rating = r.Review.Rating,
                    ReviewText = r.Review.ReviewText,
                    CreatedTime = r.Review.CreatedTime,
                    LikeCount = _repository.GetReviewLikeCount(r.Review.Id),
                    HasCurrentUserLiked = _repository.HasUserLikedReview(currentUserId, r.Review.Id)
                })
                .OrderByDescending(r => r.LikeCount)    // Sort by like count
                .ThenByDescending(r => r.CreatedTime)   // Then by date
                .ToList();

            return displayList;
        }

        /// <summary>
        /// Toggle like on a review
        /// </summary>
        public (bool isLiked, int likeCount) ToggleLike(int userId, int reviewId)
        {
            var isLiked = _repository.ToggleLike(userId, reviewId);
            var likeCount = _repository.GetReviewLikeCount(reviewId);
            return (isLiked, likeCount);
        }

        /// <summary>
        /// Check if review belongs to user
        /// </summary>
        public bool IsReviewByUser(int userId, int reviewId)
        {
            var review = _repository.GetReviewById(reviewId);
            return review != null && review.UserId == userId;
        }
    }
}

