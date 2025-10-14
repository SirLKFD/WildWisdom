using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IBookReviewRepository
    {
        /// <summary>
        /// Get review by user and book
        /// </summary>
        BookReview GetReviewByUserAndBook(int userId, int bookId);

        /// <summary>
        /// Get average rating for a book
        /// </summary>
        decimal? GetBookAverageRating(int bookId);

        /// <summary>
        /// Add or update review
        /// </summary>
        void AddOrUpdateReview(BookReview review);

        /// <summary>
        /// Get all reviews for a book
        /// </summary>
        IQueryable<BookReview> GetBookReviews(int bookId);

        /// <summary>
        /// Get reviews with user information for display
        /// </summary>
        List<(BookReview Review, string UserName, string UserProfilePicture)> GetReviewsWithUserInfo(int bookId);

        /// <summary>
        /// Get like count for a review
        /// </summary>
        int GetReviewLikeCount(int reviewId);

        /// <summary>
        /// Check if user has liked a review
        /// </summary>
        bool HasUserLikedReview(int userId, int reviewId);

        /// <summary>
        /// Toggle like on a review
        /// </summary>
        bool ToggleLike(int userId, int reviewId);

        /// <summary>
        /// Get review by ID
        /// </summary>
        BookReview GetReviewById(int reviewId);
    }
}

