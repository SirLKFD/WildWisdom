using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class ClaimPolicyRepository : BaseRepository<ClaimPolicy>, IClaimPolicyRepository
    {
        public ClaimPolicyRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public ClaimPolicy GetByOrganization(int orgId)
        {
            return this.GetDbSet<ClaimPolicy>().FirstOrDefault(x => x.OrganizationId == orgId);
        }

        public void Add(ClaimPolicy policy)
        {
            this.GetDbSet<ClaimPolicy>().Add(policy);
            UnitOfWork.SaveChanges();
        }

        public void Update(ClaimPolicy policy)
        {
            this.GetDbSet<ClaimPolicy>().Update(policy);
            UnitOfWork.SaveChanges();
        }

        public bool Exists(int orgId)
        {
            return this.GetDbSet<ClaimPolicy>().Any(x => x.OrganizationId == orgId);
        }
    }
}

