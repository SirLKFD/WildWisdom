using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IBookReviewService
    {
        /// <summary>
        /// Get or create user review for a book
        /// </summary>
        BookReviewViewModel GetOrCreateUserReview(int userId, int bookId);

        /// <summary>
        /// Save user rating and review for a book
        /// </summary>
        void SaveRating(int userId, int bookId, int? rating, string reviewText = null);

        /// <summary>
        /// Get average rating for a book
        /// </summary>
        decimal? GetBookAverageRating(int bookId);

        /// <summary>
        /// Get user's rating for a book
        /// </summary>
        int? GetUserRating(int userId, int bookId);

        /// <summary>
        /// Get book reviews for display with user info and like counts
        /// </summary>
        List<BookReviewDisplayViewModel> GetBookReviewsDisplay(int bookId, int currentUserId);

        /// <summary>
        /// Get all reviews for a book (for calculating statistics)
        /// </summary>
        List<BookReviewViewModel> GetBookReviews(int bookId);

        /// <summary>
        /// Toggle like on a review
        /// </summary>
        (bool isLiked, int likeCount) ToggleLike(int userId, int reviewId);

        /// <summary>
        /// Check if review belongs to user
        /// </summary>
        bool IsReviewByUser(int userId, int reviewId);
    }
}

