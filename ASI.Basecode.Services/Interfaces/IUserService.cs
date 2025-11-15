using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        LoginResult AuthenticateUser(string userid, string password, ref User user);
        void AddUser(UserViewModel model);
        void CreatePendingUser(UserViewModel model, string encryptedOTP, string verificationToken);
        void VerifyAndCreateUser(string email, string verificationToken);
        UserSettingsViewModel GetUserById(int id);
        UserSettingsViewModel GetUserByUserId(string userId);
        void UpdateUser(UserSettingsViewModel model, string currentUserId);
        void DeleteUser(int id);
        bool UserExists(string identifier);
    }
}
