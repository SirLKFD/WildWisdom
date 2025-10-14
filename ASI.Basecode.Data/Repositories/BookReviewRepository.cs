using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class BookReviewRepository : BaseRepository<BookReview>, IBookReviewRepository
    {
        public BookReviewRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Get review by user and book
        /// </summary>
        public BookReview GetReviewByUserAndBook(int userId, int bookId)
        {
            return GetDbSet<BookReview>()
                .FirstOrDefault(r => r.UserId == userId && r.OrgBookId == bookId);
        }

        /// <summary>
        /// Get average rating for a book
        /// </summary>
        public decimal? GetBookAverageRating(int bookId)
        {
            var reviews = GetDbSet<BookReview>()
                .Where(r => r.OrgBookId == bookId && r.Rating.HasValue)
                .ToList();

            if (!reviews.Any())
                return null;

            return (decimal)reviews.Average(r => r.Rating.Value);
        }

        /// <summary>
        /// Add or update review
        /// </summary>
        public void AddOrUpdateReview(BookReview review)
        {
            var existing = GetReviewByUserAndBook(review.UserId, review.OrgBookId);

            if (existing != null)
            {
                existing.Rating = review.Rating;
                existing.ReviewText = review.ReviewText;
                existing.UpdatedTime = DateTime.Now;
            }
            else
            {
                review.CreatedTime = DateTime.Now;
                review.UpdatedTime = DateTime.Now;
                GetDbSet<BookReview>().Add(review);
            }

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Get all reviews for a book
        /// </summary>
        public IQueryable<BookReview> GetBookReviews(int bookId)
        {
            return GetDbSet<BookReview>()
                .Where(r => r.OrgBookId == bookId);
        }

        /// <summary>
        /// Get reviews with user information for display
        /// </summary>
        public List<(BookReview Review, string UserName, string UserProfilePicture)> GetReviewsWithUserInfo(int bookId)
        {
            var reviews = GetDbSet<BookReview>()
                .Where(r => r.OrgBookId == bookId && (r.Rating.HasValue || !string.IsNullOrEmpty(r.ReviewText)))
                .Select(r => new
                {
                    Review = r,
                    UserName = r.User.Name,
                    UserProfilePicture = r.User.ProfilePicture
                })
                .ToList();

            return reviews.Select(x => (x.Review, x.UserName, x.UserProfilePicture)).ToList();
        }

        /// <summary>
        /// Get like count for a review
        /// </summary>
        public int GetReviewLikeCount(int reviewId)
        {
            return GetDbSet<BookReviewLike>()
                .Count(l => l.BookReviewId == reviewId);
        }

        /// <summary>
        /// Check if user has liked a review
        /// </summary>
        public bool HasUserLikedReview(int userId, int reviewId)
        {
            return GetDbSet<BookReviewLike>()
                .Any(l => l.UserId == userId && l.BookReviewId == reviewId);
        }

        /// <summary>
        /// Toggle like on a review (returns true if liked, false if unliked)
        /// </summary>
        public bool ToggleLike(int userId, int reviewId)
        {
            var existingLike = GetDbSet<BookReviewLike>()
                .FirstOrDefault(l => l.UserId == userId && l.BookReviewId == reviewId);

            if (existingLike != null)
            {
                // Unlike
                GetDbSet<BookReviewLike>().Remove(existingLike);
                UnitOfWork.SaveChanges();
                return false;
            }
            else
            {
                // Like
                var newLike = new BookReviewLike
                {
                    UserId = userId,
                    BookReviewId = reviewId,
                    CreatedTime = DateTime.Now
                };
                GetDbSet<BookReviewLike>().Add(newLike);
                UnitOfWork.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Get review by ID
        /// </summary>
        public BookReview GetReviewById(int reviewId)
        {
            return GetDbSet<BookReview>()
                .FirstOrDefault(r => r.Id == reviewId);
        }
    }
}

