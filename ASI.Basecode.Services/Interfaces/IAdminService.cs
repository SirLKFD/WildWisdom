using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAdminService
    {
        LoginResult AuthenticateAdmin(string username, string password, ref Admin admin);
        void AddAdmin(AdminViewModel model);
        void CreatePendingAdmin(AdminViewModel model, string encryptedOTP, string verificationToken);
        void VerifyAndCreateAdmin(string email, string verificationToken);
        bool ValidateAdminRegistration(AdminViewModel model);
        AdminSettingsViewModel GetAdminSettings(int adminId);
        void UpdateAdminSettings(AdminSettingsViewModel model);
        bool CanDeleteAccount(int adminId);
        void DeleteAdminAccount(int adminId);
        bool AdminExists(string identifier);
    }
}

