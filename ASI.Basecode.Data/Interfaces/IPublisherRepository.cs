using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IPublisherRepository
    {
        IQueryable<Publisher> GetAll();
        Publisher GetById(int id);
        Publisher GetByName(string name);
        void Add(Publisher publisher);
        void Update(Publisher publisher);
        bool Exists(string name);
    }
}

