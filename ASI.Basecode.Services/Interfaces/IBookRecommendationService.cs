using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IBookRecommendationService
    {
        /// <summary>
        /// Gets personalized book recommendations for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="take">Number of recommendations to return</param>
        /// <returns>List of recommended books</returns>
        List<OrgBooksViewModel> GetRecommendedBooks(int userId, int take = 20);

        /// <summary>
        /// Analyzes user preferences based on borrowing history
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User preference statistics</returns>
        UserPreferenceStats AnalyzeUserPreferences(int userId);

        /// <summary>
        /// Gets user preference statistics for display
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User preference stats</returns>
        UserPreferenceStats GetUserPreferenceStats(int userId);

        /// <summary>
        /// Gets books from user's favorite genres
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="take">Number of books to return</param>
        /// <returns>Books from favorite genres</returns>
        List<OrgBooksViewModel> GetBooksFromFavoriteGenres(int userId, int take = 10);

        /// <summary>
        /// Gets books from user's favorite authors
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="take">Number of books to return</param>
        /// <returns>Books from favorite authors</returns>
        List<OrgBooksViewModel> GetBooksFromFavoriteAuthors(int userId, int take = 8);

        /// <summary>
        /// Gets trending books in user's preferred categories
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="take">Number of books to return</param>
        /// <returns>Trending books in preferred categories</returns>
        List<OrgBooksViewModel> GetTrendingInPreferredCategories(int userId, int take = 10);
    }
}
