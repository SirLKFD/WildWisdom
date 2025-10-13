using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAdminService
    {
        LoginResult AuthenticateAdmin(string username, string password, ref Admin admin);
        void AddAdmin(AdminViewModel model);
        bool ValidateAdminRegistration(AdminViewModel model);
    }
}

