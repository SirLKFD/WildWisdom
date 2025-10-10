using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class RenewalPolicyRepository : BaseRepository<RenewalPolicy>, IRenewalPolicyRepository
    {
        public RenewalPolicyRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public RenewalPolicy GetByOrganization(int orgId)
        {
            return this.GetDbSet<RenewalPolicy>().FirstOrDefault(x => x.OrganizationId == orgId);
        }

        public void Add(RenewalPolicy policy)
        {
            this.GetDbSet<RenewalPolicy>().Add(policy);
            UnitOfWork.SaveChanges();
        }

        public void Update(RenewalPolicy policy)
        {
            this.GetDbSet<RenewalPolicy>().Update(policy);
            UnitOfWork.SaveChanges();
        }

        public bool Exists(int orgId)
        {
            return this.GetDbSet<RenewalPolicy>().Any(x => x.OrganizationId == orgId);
        }
    }
}

