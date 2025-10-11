using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IAdminRepository
    {
        IQueryable<Admin> GetAdmins();
        bool AdminExists(string username);
        void AddAdmin(Admin admin);
        Admin GetByUsername(string username);
    }
}

