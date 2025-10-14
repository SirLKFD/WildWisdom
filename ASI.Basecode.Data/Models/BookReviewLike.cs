using System;

namespace ASI.Basecode.Data.Models
{
    public partial class BookReviewLike
    {
        public int Id { get; set; }
        public int BookReviewId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedTime { get; set; }

        // Navigation Properties
        public virtual BookReview BookReview { get; set; }
        public virtual User User { get; set; }
    }
}

