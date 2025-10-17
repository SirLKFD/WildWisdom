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

        public bool AdminExists(string identifier)
        {
            return this.GetDbSet<Admin>().Any(x => x.AdminUsername == identifier || x.Email == identifier);
        }

        public void AddAdmin(Admin admin)
        {
            this.GetDbSet<Admin>().Add(admin);
            UnitOfWork.SaveChanges();
        }

        public Admin GetByUsername(string identifier)
        {
            return this.GetDbSet<Admin>().FirstOrDefault(x => x.AdminUsername == identifier || x.Email == identifier);
        }

        public Admin GetAdminById(int adminId)
        {
            return this.GetDbSet<Admin>().FirstOrDefault(x => x.Id == adminId);
        }

        public void UpdateAdmin(Admin admin)
        {
            this.GetDbSet<Admin>().Update(admin);
            UnitOfWork.SaveChanges();
        }

        public void DeleteAdmin(int adminId)
        {
            var admin = GetAdminById(adminId);
            if (admin != null)
            {
                this.GetDbSet<Admin>().Remove(admin);
                UnitOfWork.SaveChanges();
            }
        }
    }
}

