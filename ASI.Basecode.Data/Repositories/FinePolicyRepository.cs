using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class FinePolicyRepository : BaseRepository<FinePolicy>, IFinePolicyRepository
    {
        public FinePolicyRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public FinePolicy GetByOrganization(int orgId)
        {
            return this.GetDbSet<FinePolicy>().FirstOrDefault(x => x.OrganizationId == orgId);
        }

        public void Add(FinePolicy policy)
        {
            this.GetDbSet<FinePolicy>().Add(policy);
            UnitOfWork.SaveChanges();
        }

        public void Update(FinePolicy policy)
        {
            this.GetDbSet<FinePolicy>().Update(policy);
            UnitOfWork.SaveChanges();
        }

        public bool Exists(int orgId)
        {
            return this.GetDbSet<FinePolicy>().Any(x => x.OrganizationId == orgId);
        }
    }
}

