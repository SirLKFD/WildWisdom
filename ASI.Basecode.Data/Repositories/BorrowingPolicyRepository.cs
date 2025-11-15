using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class BorrowingPolicyRepository : BaseRepository<BorrowingPolicy>, IBorrowingPolicyRepository
    {
        public BorrowingPolicyRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public BorrowingPolicy GetByOrganization(int orgId)
        {
            return this.GetDbSet<BorrowingPolicy>().FirstOrDefault(x => x.OrganizationId == orgId);
        }

        public void Add(BorrowingPolicy policy)
        {
            this.GetDbSet<BorrowingPolicy>().Add(policy);
            UnitOfWork.SaveChanges();
        }

        public void Update(BorrowingPolicy policy)
        {
            this.GetDbSet<BorrowingPolicy>().Update(policy);
            UnitOfWork.SaveChanges();
        }

        public bool Exists(int orgId)
        {
            return this.GetDbSet<BorrowingPolicy>().Any(x => x.OrganizationId == orgId);
        }
    }
}

