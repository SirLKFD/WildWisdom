using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IClaimPolicyRepository
    {
        ClaimPolicy GetByOrganization(int orgId);
        void Add(ClaimPolicy policy);
        void Update(ClaimPolicy policy);
        bool Exists(int orgId);
    }
}

