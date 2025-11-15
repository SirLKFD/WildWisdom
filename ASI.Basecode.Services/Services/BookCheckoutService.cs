using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class BookCheckoutService : IBookCheckoutService
    {
        private readonly IBookCheckoutRepository _bookCheckoutRepository;
        private readonly IOrgBooksRepository _booksRepository;
        private readonly IMapper _mapper;

        public BookCheckoutService(
            IBookCheckoutRepository bookCheckoutRepository,
            IOrgBooksRepository booksRepository,
            IMapper mapper)
        {
            _bookCheckoutRepository = bookCheckoutRepository;
            _booksRepository = booksRepository;
            _mapper = mapper;
        }

        public List<BookCheckoutItemViewModel> GetUserCart(int userId)
        {
            var cartItems = _bookCheckoutRepository.GetUserCart(userId);
            var viewModels = new List<BookCheckoutItemViewModel>();

            foreach (var item in cartItems)
            {
                var book = _booksRepository.GetById(item.OrgBookId);
                if (book != null)
                {
                    viewModels.Add(new BookCheckoutItemViewModel
                    {
                        BookId = book.Id,
                        BookTitle = book.Title,
                        BookAuthor = book.Author,
                        BookCoverImage = book.CoverImage,
                        OrganizationName = book.Organization?.Name ?? "Library",
                        AddedAt = item.AddedAt
                    });
                }
            }

            return viewModels;
        }

        public bool AddToCart(int userId, int bookId)
        {
            try
            {
                // Check if already in cart
                if (_bookCheckoutRepository.IsInCart(userId, bookId))
                {
                    return false; // Already in cart
                }

                // Get book details
                var book = _booksRepository.GetById(bookId);
                if (book == null)
                {
                    return false; // Book not found
                }

                // Check availability
                if (book.CopiesAvailable <= 0)
                {
                    return false; // No copies available
                }

                // Add to cart
                var cartItem = new BookCheckout
                {
                    UserId = userId,
                    OrgBookId = bookId,
                    OrganizationId = book.OrganizationId,
                    AddedAt = DateTime.Now,
                    CreatedBy = "System",
                    CreatedTime = DateTime.Now,
                    UpdatedBy = "System",
                    UpdatedTime = DateTime.Now
                };

                _bookCheckoutRepository.AddToCart(cartItem);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveFromCart(int userId, int bookId)
        {
            try
            {
                _bookCheckoutRepository.RemoveFromCart(userId, bookId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ClearCart(int userId)
        {
            _bookCheckoutRepository.ClearCart(userId);
        }

        public bool IsInCart(int userId, int bookId)
        {
            return _bookCheckoutRepository.IsInCart(userId, bookId);
        }

        public int GetCartCount(int userId)
        {
            return _bookCheckoutRepository.GetUserCart(userId).Count;
        }
    }
}
