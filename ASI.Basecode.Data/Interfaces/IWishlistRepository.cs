using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IWishlistRepository
    {
        List<Wishlist> GetUserWishlist(int userId);
        void AddToWishlist(Wishlist wishlistItem);
        void RemoveFromWishlist(int userId, int bookId);
        bool IsInWishlist(int userId, int bookId);
        Wishlist GetWishlistItem(int userId, int bookId);
    }
}
