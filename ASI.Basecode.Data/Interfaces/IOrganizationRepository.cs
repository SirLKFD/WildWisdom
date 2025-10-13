using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IOrganizationRepository
    {
        IQueryable<Organization> GetOrganizations();
        bool OrganizationExists(string name);
        void AddOrganization(Organization organization);
        Organization GetById(int id);
    }
}

