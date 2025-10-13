using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public LoginResult AuthenticateUser(string userId, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers().Where(x => x.UserId == userId &&
                                                     x.Password == passwordKey &&
                                                     x.Role == 0).FirstOrDefault();

            return user != null ? LoginResult.Success : LoginResult.Failed;
        }

        public void AddUser(UserViewModel model)
        {
            var user = new User();
            if (!_repository.UserExists(model.UserId))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.Role = 0; // Set role to User
                user.CreatedTime = DateTime.Now;
                user.UpdatedTime = DateTime.Now;
                user.CreatedBy = System.Environment.UserName;
                user.UpdatedBy = System.Environment.UserName;

                _repository.AddUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }

        public UserSettingsViewModel GetUserById(int id)
        {
            var user = _repository.GetById(id);
            if (user == null)
            {
                return null;
            }
            return _mapper.Map<UserSettingsViewModel>(user);
        }

        public UserSettingsViewModel GetUserByUserId(string userId)
        {
            var user = _repository.GetByUserId(userId);
            if (user == null)
            {
                return null;
            }
            return _mapper.Map<UserSettingsViewModel>(user);
        }

        public void UpdateUser(UserSettingsViewModel model, string currentUserId)
        {
            var user = _repository.GetById(model.Id);
            if (user == null)
            {
                throw new InvalidDataException("User not found.");
            }

            // Update basic info
            user.Name = model.Name;
            user.UserId = model.UserId;
            user.OrganizationId = model.OrganizationId;
            user.ProfilePicture = model.ProfilePicture;
            user.UpdatedBy = currentUserId;
            user.UpdatedTime = DateTime.Now;

            // Update password if provided
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                user.Password = PasswordManager.EncryptPassword(model.NewPassword);
            }

            _repository.UpdateUser(user);
        }

        public void DeleteUser(int id)
        {
            var user = _repository.GetById(id);
            if (user == null)
            {
                throw new InvalidDataException("User not found.");
            }

            _repository.DeleteUser(id);
        }
    }
}
