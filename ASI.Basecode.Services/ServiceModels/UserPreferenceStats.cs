using System.Collections.Generic;

namespace ASI.Basecode.Services.ServiceModels
{
    public class UserPreferenceStats
    {
        public int FictionCount { get; set; }
        public int NonFictionCount { get; set; }
        public Dictionary<string, int> GenreCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> AuthorCounts { get; set; } = new Dictionary<string, int>();
        public string PreferredCategory { get; set; }
        public List<string> TopGenres { get; set; } = new List<string>();
        public List<string> FavoriteAuthors { get; set; } = new List<string>();
        public int TotalBorrowedBooks { get; set; }
        public bool HasBorrowingHistory => TotalBorrowedBooks > 0;

        /// <summary>
        /// Gets the category preference percentage
        /// </summary>
        public double FictionPercentage => TotalBorrowedBooks > 0 ? (double)FictionCount / TotalBorrowedBooks * 100 : 0;
        public double NonFictionPercentage => TotalBorrowedBooks > 0 ? (double)NonFictionCount / TotalBorrowedBooks * 100 : 0;

        /// <summary>
        /// Gets the most preferred genre
        /// </summary>
        public string TopGenre => TopGenres.Count > 0 ? TopGenres[0] : "General";
    }
}
