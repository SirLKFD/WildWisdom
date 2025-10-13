using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class OrgBooksRepository : BaseRepository<OrgBooks>, IOrgBooksRepository
    {
        public OrgBooksRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<OrgBooks> GetAll()
        {
            return this.GetDbSet<OrgBooks>()
                .Include(x => x.Organization);
        }

        public IQueryable<OrgBooks> GetBooksByOrganization(int orgId)
        {
            return this.GetDbSet<OrgBooks>()
                .Include(x => x.Organization)
                .Where(x => x.OrganizationId == orgId && x.IsActive);
        }

        public IQueryable<OrgBooks> GetAllBooksByOrganization(int orgId)
        {
            return this.GetDbSet<OrgBooks>()
                .Include(x => x.Organization)
                .Where(x => x.OrganizationId == orgId);
        }

        public IQueryable<OrgBooks> GetAvailableBooks(int orgId)
        {
            return this.GetDbSet<OrgBooks>()
                .Include(x => x.Organization)
                .Where(x => x.OrganizationId == orgId && x.IsActive && x.CopiesAvailable > 0);
        }

        public void AddBook(OrgBooks book)
        {
            this.GetDbSet<OrgBooks>().Add(book);
            UnitOfWork.SaveChanges();
        }

        public void UpdateBook(OrgBooks book)
        {
            this.GetDbSet<OrgBooks>().Update(book);
            UnitOfWork.SaveChanges();
        }

        public void DecrementCopies(int bookId)
        {
            var book = this.GetDbSet<OrgBooks>().FirstOrDefault(x => x.Id == bookId);
            if (book != null && book.CopiesAvailable > 0)
            {
                book.CopiesAvailable--;
                UnitOfWork.SaveChanges();
            }
        }

        public void IncrementCopies(int bookId)
        {
            var book = this.GetDbSet<OrgBooks>().FirstOrDefault(x => x.Id == bookId);
            if (book != null && book.CopiesAvailable < book.CopiesTotal)
            {
                book.CopiesAvailable++;
                UnitOfWork.SaveChanges();
            }
        }

        public OrgBooks GetById(int id)
        {
            return this.GetDbSet<OrgBooks>()
                .Include(x => x.Organization)
                .FirstOrDefault(x => x.Id == id);
        }

        public bool BookExists(string isbn, int orgId)
        {
            return this.GetDbSet<OrgBooks>().Any(x => x.ISBN == isbn && x.OrganizationId == orgId);
        }

        public void DeleteBook(int id)
        {
            var book = this.GetDbSet<OrgBooks>().FirstOrDefault(x => x.Id == id);
            if (book != null)
            {
                this.GetDbSet<OrgBooks>().Remove(book);
                UnitOfWork.SaveChanges();
            }
        }
    }
}


