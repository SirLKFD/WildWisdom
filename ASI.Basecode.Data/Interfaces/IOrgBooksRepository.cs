using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IOrgBooksRepository
    {
        IQueryable<OrgBooks> GetAll();
        IQueryable<OrgBooks> GetBooksByOrganization(int orgId);
        IQueryable<OrgBooks> GetAllBooksByOrganization(int orgId);
        IQueryable<OrgBooks> GetAvailableBooks(int orgId);
        void AddBook(OrgBooks book);
        void UpdateBook(OrgBooks book);
        void DeleteBook(int id);
        void DecrementCopies(int bookId);
        void IncrementCopies(int bookId);
        OrgBooks GetById(int id);
        bool BookExists(string isbn, int orgId);
        int GetActiveBorrowsCount(int bookId);
    }
}


