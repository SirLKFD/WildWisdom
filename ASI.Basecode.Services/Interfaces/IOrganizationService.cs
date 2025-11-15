using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IOrganizationService
    {
        List<OrganizationViewModel> GetAllOrganizations();
        bool ValidateOrganization(string name);
        void AddOrganization(OrganizationViewModel model);
        OrganizationViewModel GetOrganizationById(int id);
    }
}

