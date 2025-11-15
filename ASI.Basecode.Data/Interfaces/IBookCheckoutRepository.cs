using ASI.Basecode.Data.Models;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IBookCheckoutRepository
    {
        List<BookCheckout> GetUserCart(int userId);
        void AddToCart(BookCheckout item);
        void RemoveFromCart(int userId, int bookId);
        void ClearCart(int userId);
        bool IsInCart(int userId, int bookId);
        BookCheckout GetCartItem(int userId, int bookId);
    }
}
