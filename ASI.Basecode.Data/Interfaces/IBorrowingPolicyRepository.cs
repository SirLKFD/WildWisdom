using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IBorrowingPolicyRepository
    {
        BorrowingPolicy GetByOrganization(int orgId);
        void Add(BorrowingPolicy policy);
        void Update(BorrowingPolicy policy);
        bool Exists(int orgId);
    }
}

