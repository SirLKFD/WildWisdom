using System;

namespace ASI.Basecode.Services.ServiceModels
{
    public class CartItemViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public string BookCoverImage { get; set; }
        public string OrganizationName { get; set; }
        public DateTime AddedToCartAt { get; set; }
    }
}
