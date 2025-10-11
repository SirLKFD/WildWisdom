using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class AdminRepository : BaseRepository<Admin>, IAdminRepository
    {
        public AdminRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<Admin> GetAdmins()
        {
            return this.GetDbSet<Admin>();
        }

        public bool AdminExists(string username)
        {
            return this.GetDbSet<Admin>().Any(x => x.AdminUsername == username);
        }

        public void AddAdmin(Admin admin)
        {
            this.GetDbSet<Admin>().Add(admin);
            UnitOfWork.SaveChanges();
        }

        public Admin GetByUsername(string username)
        {
            return this.GetDbSet<Admin>().FirstOrDefault(x => x.AdminUsername == username);
        }
    }
}

