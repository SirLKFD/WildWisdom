using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class BookCheckoutRepository : BaseRepository<BookCheckout>, IBookCheckoutRepository
    {
        public BookCheckoutRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public List<BookCheckout> GetUserCart(int userId)
        {
            return this.GetDbSet<BookCheckout>()
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.AddedAt)
                .ToList();
        }

        public void AddToCart(BookCheckout item)
        {
            this.GetDbSet<BookCheckout>().Add(item);
            UnitOfWork.SaveChanges();
        }

        public void RemoveFromCart(int userId, int bookId)
        {
            var item = this.GetDbSet<BookCheckout>()
                .FirstOrDefault(x => x.UserId == userId && x.OrgBookId == bookId);
            
            if (item != null)
            {
                this.GetDbSet<BookCheckout>().Remove(item);
                UnitOfWork.SaveChanges();
            }
        }

        public void ClearCart(int userId)
        {
            var items = this.GetDbSet<BookCheckout>()
                .Where(x => x.UserId == userId)
                .ToList();
            
            if (items.Any())
            {
                this.GetDbSet<BookCheckout>().RemoveRange(items);
                UnitOfWork.SaveChanges();
            }
        }

        public bool IsInCart(int userId, int bookId)
        {
            return this.GetDbSet<BookCheckout>()
                .Any(x => x.UserId == userId && x.OrgBookId == bookId);
        }

        public BookCheckout GetCartItem(int userId, int bookId)
        {
            return this.GetDbSet<BookCheckout>()
                .FirstOrDefault(x => x.UserId == userId && x.OrgBookId == bookId);
        }
    }
}
