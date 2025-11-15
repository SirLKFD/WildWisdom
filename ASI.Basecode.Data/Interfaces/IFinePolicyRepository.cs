using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IFinePolicyRepository
    {
        FinePolicy GetByOrganization(int orgId);
        void Add(FinePolicy policy);
        void Update(FinePolicy policy);
        bool Exists(int orgId);
    }
}

