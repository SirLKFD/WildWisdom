using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IOrgBooksService
    {
        List<OrgBooksViewModel> GetAllBooks();
        List<OrgBooksViewModel> GetBookCatalog(int orgId);
        List<OrgBooksViewModel> GetAvailableBooks(int orgId);
        void AddBook(OrgBooksViewModel model);
        void UpdateBook(OrgBooksViewModel model);
        void DeleteBook(int id);
        OrgBooksViewModel GetBookById(int id);
        bool ValidateBookAvailability(int bookId);
        void ReserveCopy(int bookId);
        void ReleaseCopy(int bookId);
    }
}


