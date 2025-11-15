using System.Collections.Generic;

namespace ASI.Basecode.Services.ServiceModels
{
    public class BookRecommendationViewModel
    {
        public List<OrgBooksViewModel> RecommendedBooks { get; set; } = new List<OrgBooksViewModel>();
        public UserPreferenceStats PreferenceStats { get; set; }
        public string TopCategory { get; set; }
        public List<string> TopGenres { get; set; } = new List<string>();
        public List<string> FavoriteAuthors { get; set; } = new List<string>();
        public int TotalBorrowedBooks { get; set; }
        
        // Specific recommendation categories
        public List<OrgBooksViewModel> FavoriteGenreBooks { get; set; } = new List<OrgBooksViewModel>();
        public List<OrgBooksViewModel> FavoriteAuthorBooks { get; set; } = new List<OrgBooksViewModel>();
        public List<OrgBooksViewModel> TrendingBooks { get; set; } = new List<OrgBooksViewModel>();
        public List<OrgBooksViewModel> CategoryBooks { get; set; } = new List<OrgBooksViewModel>();

        // Search and pagination
        public string SearchTerm { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public bool HasSearchResults { get; set; }

        // UI state
        public bool ShowEmptyState => !PreferenceStats.HasBorrowingHistory && RecommendedBooks.Count == 0;
        public string EmptyStateMessage => PreferenceStats.HasBorrowingHistory 
            ? "No recommendations found. Try different search terms."
            : "Start borrowing books to get personalized recommendations!";
    }
}
