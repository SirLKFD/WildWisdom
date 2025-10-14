using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class OrganizationRepository : BaseRepository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<Organization> GetOrganizations()
        {
            return this.GetDbSet<Organization>();
        }

        public bool OrganizationExists(string name)
        {
            return this.GetDbSet<Organization>().Any(x => x.Name == name);
        }

        public void AddOrganization(Organization organization)
        {
            this.GetDbSet<Organization>().Add(organization);
            UnitOfWork.SaveChanges();
        }

        public Organization GetById(int id)
        {
            return this.GetDbSet<Organization>().FirstOrDefault(x => x.Id == id);
        }

        public Organization GetOrganizationById(int id)
        {
            return this.GetDbSet<Organization>().FirstOrDefault(x => x.Id == id);
        }

        public void UpdateOrganization(Organization organization)
        {
            this.GetDbSet<Organization>().Update(organization);
            UnitOfWork.SaveChanges();
        }

        public void DeleteOrganization(int organizationId)
        {
            var organization = GetOrganizationById(organizationId);
            if (organization != null)
            {
                // Cascade delete will handle OrgBooks
                this.GetDbSet<Organization>().Remove(organization);
                UnitOfWork.SaveChanges();
            }
        }
    }
}

