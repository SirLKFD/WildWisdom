using System;

namespace ASI.Basecode.Services.ServiceModels
{
    public class WishlistItemViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public string BookCoverImage { get; set; }
        public string OrganizationName { get; set; }
        public DateTime AddedAt { get; set; }
        public int CopiesAvailable { get; set; }
        public bool IsAvailable => CopiesAvailable > 0;
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
