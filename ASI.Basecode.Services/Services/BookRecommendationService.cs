using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class BookRecommendationService : ServiceBase, IBookRecommendationService
    {
        private readonly IBorrowTicketRepository _ticketRepository;
        private readonly IOrgBooksRepository _booksRepository;
        private readonly IBookReviewRepository _reviewRepository;

        public BookRecommendationService(IBorrowTicketRepository ticketRepository, IOrgBooksRepository booksRepository, IBookReviewRepository reviewRepository, ILoggerFactory loggerFactory) 
            : base(loggerFactory)
        {
            _ticketRepository = ticketRepository;
            _booksRepository = booksRepository;
            _reviewRepository = reviewRepository;
        }

        public List<OrgBooksViewModel> GetRecommendedBooks(int userId, int take = 20)
        {
            var preferences = AnalyzeUserPreferences(userId);
            
            if (!preferences.HasBorrowingHistory)
            {
                // Return popular books as fallback for new users
                return GetPopularBooksFallback(take);
            }

            var recommendations = new List<OrgBooksViewModel>();
            var orgId = GetUserOrganizationId(userId);
            var allBooks = _booksRepository.GetBooksByOrganization(orgId).Where(b => b.IsActive).ToList();

            // 1. Get books from favorite genres (40%)
            var genreBooks = GetBooksFromFavoriteGenres(userId, (int)(take * 0.4));
            recommendations.AddRange(genreBooks);

            // 2. Get books from favorite category (30%)
            var categoryBooks = GetBooksFromPreferredCategory(preferences, allBooks, (int)(take * 0.3));
            recommendations.AddRange(categoryBooks);

            // 3. Get books from favorite authors (20%)
            var authorBooks = GetBooksFromFavoriteAuthors(userId, (int)(take * 0.2));
            recommendations.AddRange(authorBooks);

            // 4. Get trending books as wildcards (10%)
            var trendingBooks = GetTrendingInPreferredCategories(userId, (int)(take * 0.1));
            recommendations.AddRange(trendingBooks);

            // Remove duplicates and books already borrowed
            var borrowedBookIds = GetBorrowedBookIds(userId);
            var uniqueRecommendations = recommendations
                .Where(b => !borrowedBookIds.Contains(b.Id))
                .GroupBy(b => b.Id)
                .Select(g => g.First())
                .Take(take)
                .ToList();

            return uniqueRecommendations;
        }

        public UserPreferenceStats AnalyzeUserPreferences(int userId)
        {
            var stats = new UserPreferenceStats();
            var tickets = _ticketRepository.GetTicketsByUser(userId).Where(t => t.Status == TicketStatus.Returned || t.Status == TicketStatus.NotYetReturned);

            if (tickets == null || !tickets.Any())
            {
                return stats;
            }

            var borrowedBookIds = tickets.Select(t => t.OrgBookId).Distinct().ToList();
            var books = new List<OrgBooks>();
            foreach (var bookId in borrowedBookIds)
            {
                var book = _booksRepository.GetById(bookId);
                if (book != null) books.Add(book);
            }

            stats.TotalBorrowedBooks = borrowedBookIds.Count();

            // Analyze categories
            stats.FictionCount = books.Count(b => b.Category?.Equals("Fiction", StringComparison.OrdinalIgnoreCase) == true);
            stats.NonFictionCount = books.Count(b => b.Category?.Equals("Non-Fiction", StringComparison.OrdinalIgnoreCase) == true);
            
            stats.PreferredCategory = stats.FictionCount >= stats.NonFictionCount ? "Fiction" : "Non-Fiction";

            // Analyze genres
            var genreGroups = books.Where(b => !string.IsNullOrEmpty(b.Genre))
                                  .GroupBy(b => b.Genre)
                                  .OrderByDescending(g => g.Count())
                                  .ToList();

            stats.GenreCounts = genreGroups.ToDictionary(g => g.Key, g => g.Count());
            stats.TopGenres = genreGroups.Take(3).Select(g => g.Key).ToList();

            // Analyze authors
            var authorGroups = books.Where(b => !string.IsNullOrEmpty(b.Author))
                                   .GroupBy(b => b.Author)
                                   .OrderByDescending(g => g.Count())
                                   .ToList();

            stats.AuthorCounts = authorGroups.ToDictionary(g => g.Key, g => g.Count());
            stats.FavoriteAuthors = authorGroups.Take(3).Select(g => g.Key).ToList();

            return stats;
        }

        public UserPreferenceStats GetUserPreferenceStats(int userId)
        {
            return AnalyzeUserPreferences(userId);
        }

        public List<OrgBooksViewModel> GetBooksFromFavoriteGenres(int userId, int take = 10)
        {
            var preferences = AnalyzeUserPreferences(userId);
            if (!preferences.HasBorrowingHistory || preferences.TopGenres.Count == 0)
            {
                return new List<OrgBooksViewModel>();
            }

            var orgId = GetUserOrganizationId(userId);
            var topGenre = preferences.TopGenres[0];
            var borrowedBookIds = GetBorrowedBookIds(userId);

            var books = _booksRepository.GetBooksByOrganization(orgId).Where(b => b.IsActive)
                .Where(b => b.Genre == topGenre && !borrowedBookIds.Contains(b.Id))
                .OrderByDescending(b => b.CreatedTime)
                .Take(take)
                .ToList();

            var result = new List<OrgBooksViewModel>();
            foreach (var book in books)
            {
                var rating = _reviewRepository.GetBookAverageRating(book.Id);
                result.Add(new OrgBooksViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    CoverImage = book.CoverImage,
                    AverageRating = rating,
                    Category = book.Category,
                    Genre = book.Genre,
                    Description = book.Description,
                    CopiesAvailable = book.CopiesAvailable
                });
            }
            
            return result.OrderByDescending(b => b.AverageRating ?? 0).ToList();
        }

        public List<OrgBooksViewModel> GetBooksFromFavoriteAuthors(int userId, int take = 8)
        {
            var preferences = AnalyzeUserPreferences(userId);
            if (!preferences.HasBorrowingHistory || preferences.FavoriteAuthors.Count == 0)
            {
                return new List<OrgBooksViewModel>();
            }

            var orgId = GetUserOrganizationId(userId);
            var favoriteAuthors = preferences.FavoriteAuthors;
            var borrowedBookIds = GetBorrowedBookIds(userId);

            var books = _booksRepository.GetBooksByOrganization(orgId).Where(b => b.IsActive)
                .Where(b => favoriteAuthors.Contains(b.Author) && !borrowedBookIds.Contains(b.Id))
                .OrderByDescending(b => b.CreatedTime)
                .Take(take)
                .ToList();

            var result = new List<OrgBooksViewModel>();
            foreach (var book in books)
            {
                var rating = _reviewRepository.GetBookAverageRating(book.Id);
                result.Add(new OrgBooksViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    CoverImage = book.CoverImage,
                    AverageRating = rating,
                    Category = book.Category,
                    Genre = book.Genre,
                    Description = book.Description,
                    CopiesAvailable = book.CopiesAvailable
                });
            }
            
            return result.OrderByDescending(b => b.AverageRating ?? 0).ToList();
        }

        public List<OrgBooksViewModel> GetTrendingInPreferredCategories(int userId, int take = 10)
        {
            var preferences = AnalyzeUserPreferences(userId);
            var orgId = GetUserOrganizationId(userId);
            var borrowedBookIds = GetBorrowedBookIds(userId);

            var query = _booksRepository.GetBooksByOrganization(orgId).Where(b => b.IsActive && !borrowedBookIds.Contains(b.Id));

            // Filter by preferred category if user has history
            if (preferences.HasBorrowingHistory)
            {
                query = query.Where(b => b.Category == preferences.PreferredCategory);
            }

            var books = query
                .OrderByDescending(b => b.CreatedTime)
                .Take(take)
                .ToList();

            var result = new List<OrgBooksViewModel>();
            foreach (var book in books)
            {
                var rating = _reviewRepository.GetBookAverageRating(book.Id);
                result.Add(new OrgBooksViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    CoverImage = book.CoverImage,
                    AverageRating = rating,
                    Category = book.Category,
                    Genre = book.Genre,
                    Description = book.Description,
                    CopiesAvailable = book.CopiesAvailable
                });
            }
            
            return result.OrderByDescending(b => b.AverageRating ?? 0).ToList();
        }

        private List<OrgBooksViewModel> GetBooksFromPreferredCategory(UserPreferenceStats preferences, List<OrgBooks> allBooks, int take)
        {
            if (!preferences.HasBorrowingHistory)
            {
                return new List<OrgBooksViewModel>();
            }

            var borrowedBookIds = GetBorrowedBookIds(GetUserIdFromPreferences());
            
            var books = allBooks
                .Where(b => b.Category == preferences.PreferredCategory && !borrowedBookIds.Contains(b.Id))
                .OrderByDescending(b => b.CreatedTime)
                .Take(take)
                .ToList();

            var result = new List<OrgBooksViewModel>();
            foreach (var book in books)
            {
                var rating = _reviewRepository.GetBookAverageRating(book.Id);
                result.Add(new OrgBooksViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    CoverImage = book.CoverImage,
                    AverageRating = rating,
                    Category = book.Category,
                    Genre = book.Genre,
                    Description = book.Description,
                    CopiesAvailable = book.CopiesAvailable
                });
            }
            
            return result.OrderByDescending(b => b.AverageRating ?? 0).ToList();
        }

        private List<OrgBooksViewModel> GetPopularBooksFallback(int take)
        {
            // This would need to be implemented with actual organization ID
            // For now, return empty list - will be handled in controller
            return new List<OrgBooksViewModel>();
        }

        private List<int> GetBorrowedBookIds(int userId)
        {
            var tickets = _ticketRepository.GetTicketsByUser(userId).Where(t => t.Status == TicketStatus.Returned || t.Status == TicketStatus.NotYetReturned);
            return tickets?.Select(t => t.OrgBookId).ToList() ?? new List<int>();
        }

        private int GetUserOrganizationId(int userId)
        {
            // This would need to be implemented based on your user model
            // For now, return 1 as default - should be passed from controller
            return 1;
        }

        private int GetUserIdFromPreferences()
        {
            // This is a placeholder - in real implementation, userId should be passed
            return 1;
        }
    }
}
