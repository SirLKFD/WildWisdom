using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IWishlistService
    {
        List<WishlistItemViewModel> GetUserWishlist(int userId);
        bool AddToWishlist(int userId, int bookId);
        bool RemoveFromWishlist(int userId, int bookId);
        bool ToggleWishlist(int userId, int bookId);
        bool IsInWishlist(int userId, int bookId);
    }
}
