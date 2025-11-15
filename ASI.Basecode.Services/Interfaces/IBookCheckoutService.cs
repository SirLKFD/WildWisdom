using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IBookCheckoutService
    {
        List<BookCheckoutItemViewModel> GetUserCart(int userId);
        bool AddToCart(int userId, int bookId);
        bool RemoveFromCart(int userId, int bookId);
        void ClearCart(int userId);
        bool IsInCart(int userId, int bookId);
        int GetCartCount(int userId);
    }
}
