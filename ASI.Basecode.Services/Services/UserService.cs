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
        private readonly IPendingUserRegistrationRepository _pendingRepository;

        public UserService(IUserRepository repository, IMapper mapper, IPendingUserRegistrationRepository pendingRepository)
        {
            _mapper = mapper;
            _repository = repository;
            _pendingRepository = pendingRepository;
        }

        public LoginResult AuthenticateUser(string userId, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers().Where(x => (x.UserId == userId || x.Email == userId) &&
                                                     x.Password == passwordKey &&
                                                     x.Role == 0).FirstOrDefault();

            if (user != null && !user.IsVerified)
            {
                return LoginResult.Failed; // User exists but not verified
            }

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
            user.Email = model.Email;
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

        public void CreatePendingUser(UserViewModel model, string encryptedOTP, string verificationToken)
        {
            // Check if email already exists in pending or actual users
            if (_pendingRepository.EmailExists(model.Email))
            {
                throw new InvalidDataException("Email already registered. Please check your email for verification.");
            }

            if (_repository.UserExists(model.Email))
            {
                throw new InvalidDataException("Email already registered.");
            }

            var pendingUser = new Data.Models.PendingUserRegistration
            {
                EmailId = model.Email,
                Name = model.UserId, // Store user-chosen UserId in Name field temporarily
                PasswordHash = PasswordManager.EncryptPassword(model.Password), // Store encrypted user password
                EncryptedOTP = encryptedOTP, // Store encrypted OTP
                VerificationToken = verificationToken,
                TokenExpiration = DateTime.Now.AddMinutes(30),
                IsVerified = false,
                Role = 0, // User role
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now
            };

            _pendingRepository.AddPending(pendingUser);
        }

        public void VerifyAndCreateUser(string email, string verificationToken)
        {
            var pending = _pendingRepository.GetByEmail(email);
            
            if (pending == null)
            {
                throw new InvalidDataException("Pending registration not found.");
            }

            if (pending.VerificationToken != verificationToken)
            {
                throw new InvalidDataException("Invalid verification token.");
            }

            if (pending.TokenExpiration < DateTime.Now)
            {
                _pendingRepository.DeletePending(pending.Id);
                throw new InvalidDataException("Verification token has expired. Please register again.");
            }

            // Create actual user
            var user = new User
            {
                UserId = pending.Name, // Name field stores the user-chosen UserId from registration
                Email = pending.EmailId,
                Name = pending.Name,
                Password = pending.PasswordHash, // Use the encrypted password from registration
                Role = 0,
                OrganizationId = pending.OrganizationId,
                IsVerified = true,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                CreatedBy = System.Environment.UserName,
                UpdatedBy = System.Environment.UserName
            };

            _repository.AddUser(user);
            
            // Delete pending registration
            _pendingRepository.DeletePending(pending.Id);
        }

        public bool UserExists(string identifier)
        {
            return _repository.UserExists(identifier);
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
