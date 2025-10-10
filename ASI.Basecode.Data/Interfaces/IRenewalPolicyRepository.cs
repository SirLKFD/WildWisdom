using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IRenewalPolicyRepository
    {
        RenewalPolicy GetByOrganization(int orgId);
        void Add(RenewalPolicy policy);
        void Update(RenewalPolicy policy);
        bool Exists(int orgId);
    }
}

