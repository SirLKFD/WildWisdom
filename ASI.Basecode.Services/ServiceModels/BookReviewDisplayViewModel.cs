using System;

namespace ASI.Basecode.Services.ServiceModels
{
    public class BookReviewDisplayViewModel
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserProfilePicture { get; set; }
        public int? Rating { get; set; }
        public string ReviewText { get; set; }
        public DateTime CreatedTime { get; set; }
        public int LikeCount { get; set; }
        public bool HasCurrentUserLiked { get; set; }
    }
}

