using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class PublisherRepository : BaseRepository<Publisher>, IPublisherRepository
    {
        public PublisherRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<Publisher> GetAll()
        {
            return this.GetDbSet<Publisher>();
        }

        public Publisher GetById(int id)
        {
            return this.GetDbSet<Publisher>().FirstOrDefault(x => x.Id == id);
        }

        public Publisher GetByName(string name)
        {
            return this.GetDbSet<Publisher>().FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
        }

        public void Add(Publisher publisher)
        {
            this.GetDbSet<Publisher>().Add(publisher);
            UnitOfWork.SaveChanges();
        }

        public void Update(Publisher publisher)
        {
            this.GetDbSet<Publisher>().Update(publisher);
            UnitOfWork.SaveChanges();
        }

        public bool Exists(string name)
        {
            return this.GetDbSet<Publisher>().Any(x => x.Name.ToLower() == name.ToLower());
        }
    }
}

