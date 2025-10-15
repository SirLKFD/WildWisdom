using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class WishlistRepository : BaseRepository<Wishlist>, IWishlistRepository
    {
        public WishlistRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public List<Wishlist> GetUserWishlist(int userId)
        {
            return this.GetDbSet<Wishlist>()
                .Include(x => x.OrgBook)
                    .ThenInclude(ob => ob.Organization)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AddedAt)
                .ToList();
        }

        public void AddToWishlist(Wishlist wishlistItem)
        {
            this.GetDbSet<Wishlist>().Add(wishlistItem);
            UnitOfWork.SaveChanges();
        }

        public void RemoveFromWishlist(int userId, int bookId)
        {
            var item = this.GetDbSet<Wishlist>()
                .FirstOrDefault(x => x.UserId == userId && x.OrgBookId == bookId);
            
            if (item != null)
            {
                this.GetDbSet<Wishlist>().Remove(item);
                UnitOfWork.SaveChanges();
            }
        }

        public bool IsInWishlist(int userId, int bookId)
        {
            return this.GetDbSet<Wishlist>()
                .Any(x => x.UserId == userId && x.OrgBookId == bookId);
        }

        public Wishlist GetWishlistItem(int userId, int bookId)
        {
            return this.GetDbSet<Wishlist>()
                .FirstOrDefault(x => x.UserId == userId && x.OrgBookId == bookId);
        }
    }
}
