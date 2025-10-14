using System;

namespace ASI.Basecode.Data.Models
{
    public partial class BookReview
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OrgBookId { get; set; }
        public int? Rating { get; set; }
        public string ReviewText { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }

        // Navigation Properties
        public virtual User User { get; set; }
        public virtual OrgBooks OrgBook { get; set; }
    }
}

