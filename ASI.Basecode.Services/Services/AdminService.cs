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
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IMapper _mapper;
        private readonly IPendingUserRegistrationRepository _pendingRepository;

        public AdminService(IAdminRepository adminRepository, IOrganizationRepository organizationRepository, IMapper mapper, IPendingUserRegistrationRepository pendingRepository)
        {
            _mapper = mapper;
            _adminRepository = adminRepository;
            _organizationRepository = organizationRepository;
            _pendingRepository = pendingRepository;
        }

        public LoginResult AuthenticateAdmin(string username, string password, ref Admin admin)
        {
            admin = new Admin();
            var passwordKey = PasswordManager.EncryptPassword(password);
            admin = _adminRepository.GetAdmins().Where(x => (x.AdminUsername == username || x.Email == username) &&
                                                     x.AdminPassword == passwordKey).FirstOrDefault();

            if (admin != null && !admin.IsVerified)
            {
                return LoginResult.Failed; // Admin exists but not verified
            }

            return admin != null ? LoginResult.Success : LoginResult.Failed;
        }

        public void AddAdmin(AdminViewModel model)
        {
            if (!ValidateAdminRegistration(model))
            {
                throw new InvalidDataException("Organization name already exists.");
            }

            if (_adminRepository.AdminExists(model.AdminUsername))
            {
                throw new InvalidDataException("Admin username already exists.");
            }

            // Create or get organization
            Organization organization;
            if (_organizationRepository.OrganizationExists(model.OrganizationName))
            {
                throw new InvalidDataException("Organization name already exists.");
            }
            else
            {
                organization = new Organization
                {
                    Name = model.OrganizationName,
                    Type = model.OrganizationType,
                    CreatedTime = DateTime.Now,
                    UpdatedTime = DateTime.Now,
                    CreatedBy = System.Environment.UserName,
                    UpdatedBy = System.Environment.UserName
                };
                _organizationRepository.AddOrganization(organization);
            }

            // Create admin
            var admin = new Admin
            {
                AdminUsername = model.AdminUsername,
                AdminPassword = PasswordManager.EncryptPassword(model.AdminPassword),
                OrganizationId = organization.Id,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                CreatedBy = System.Environment.UserName,
                UpdatedBy = System.Environment.UserName
            };

            _adminRepository.AddAdmin(admin);
        }

        public bool ValidateAdminRegistration(AdminViewModel model)
        {
            // Check if organization name already exists
            return !_organizationRepository.OrganizationExists(model.OrganizationName);
        }

        public AdminSettingsViewModel GetAdminSettings(int adminId)
        {
            var admin = _adminRepository.GetAdminById(adminId);
            if (admin == null)
            {
                throw new InvalidDataException("Admin not found.");
            }

            var organization = _organizationRepository.GetOrganizationById(admin.OrganizationId.Value);
            if (organization == null)
            {
                throw new InvalidDataException("Organization not found.");
            }

            return new AdminSettingsViewModel
            {
                AdminId = admin.Id,
                AdminUsername = admin.AdminUsername,
                Email = admin.Email,
                ProfilePicture = admin.ProfilePicture,
                OrganizationId = organization.Id,
                OrganizationName = organization.Name,
                OrganizationType = organization.Type,
                ContactNumber = organization.ContactNumber,
                Address = organization.Address
            };
        }

        public void UpdateAdminSettings(AdminSettingsViewModel model)
        {
            var admin = _adminRepository.GetAdminById(model.AdminId);
            if (admin == null)
            {
                throw new InvalidDataException("Admin not found.");
            }

            var organization = _organizationRepository.GetOrganizationById(model.OrganizationId);
            if (organization == null)
            {
                throw new InvalidDataException("Organization not found.");
            }

            // Update admin
            admin.AdminUsername = model.AdminUsername;
            admin.Email = model.Email;
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                admin.AdminPassword = PasswordManager.EncryptPassword(model.NewPassword);
            }
            if (!string.IsNullOrEmpty(model.ProfilePicture))
            {
                admin.ProfilePicture = model.ProfilePicture;
            }
            admin.UpdatedBy = System.Environment.UserName;
            admin.UpdatedTime = DateTime.Now;

            _adminRepository.UpdateAdmin(admin);

            // Update organization
            organization.Name = model.OrganizationName;
            organization.Type = model.OrganizationType;
            organization.ContactNumber = model.ContactNumber;
            organization.Address = model.Address;
            organization.UpdatedBy = System.Environment.UserName;
            organization.UpdatedTime = DateTime.Now;

            _organizationRepository.UpdateOrganization(organization);
        }

        public bool CanDeleteAccount(int adminId)
        {
            // TODO: Check for pending borrows, late returns, etc.
            // For now, return true as validation will be implemented later
            return true;
        }

        public void CreatePendingAdmin(AdminViewModel model, string encryptedOTP, string verificationToken)
        {
            // Check if email already exists in pending registrations
            if (_pendingRepository.EmailExists(model.Email))
            {
                throw new InvalidDataException("Email already registered. Please check your email for verification.");
            }

            // Check if admin username already exists
            if (_adminRepository.AdminExists(model.AdminUsername))
            {
                throw new InvalidDataException("Admin username already exists.");
            }

            // Check if organization name already exists
            if (_organizationRepository.OrganizationExists(model.OrganizationName))
            {
                throw new InvalidDataException("Organization name already exists.");
            }

            var pendingAdmin = new Data.Models.PendingUserRegistration
            {
                EmailId = model.Email,
                Name = model.AdminUsername, // Store username in Name field
                PasswordHash = PasswordManager.EncryptPassword(model.AdminPassword), // Store encrypted admin password
                EncryptedOTP = encryptedOTP, // Store encrypted OTP
                VerificationToken = verificationToken,
                TokenExpiration = DateTime.Now.AddMinutes(30),
                IsVerified = false,
                Role = 1, // Admin role
                OrganizationName = model.OrganizationName,
                OrganizationType = model.OrganizationType,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now
            };

            _pendingRepository.AddPending(pendingAdmin);
        }

        public void VerifyAndCreateAdmin(string email, string verificationToken)
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

            // Create organization
            var organization = new Organization
            {
                Name = pending.OrganizationName,
                Type = pending.OrganizationType,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                CreatedBy = System.Environment.UserName,
                UpdatedBy = System.Environment.UserName
            };
            _organizationRepository.AddOrganization(organization);

            // Create admin
            var admin = new Admin
            {
                AdminUsername = pending.Name, // Name field contains username
                Email = pending.EmailId,
                AdminPassword = pending.PasswordHash, // Use the encrypted password from registration
                OrganizationId = organization.Id,
                IsVerified = true,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                CreatedBy = System.Environment.UserName,
                UpdatedBy = System.Environment.UserName
            };

            _adminRepository.AddAdmin(admin);
            
            // Delete pending registration
            _pendingRepository.DeletePending(pending.Id);
        }

        public void DeleteAdminAccount(int adminId)
        {
            var admin = _adminRepository.GetAdminById(adminId);
            if (admin == null)
            {
                throw new InvalidDataException("Admin not found.");
            }

            if (!CanDeleteAccount(adminId))
            {
                throw new InvalidOperationException("Cannot delete account with pending transactions.");
            }

            var organizationId = admin.OrganizationId.Value;

            // Delete admin first
            _adminRepository.DeleteAdmin(adminId);

            // Delete organization and cascade to books
            _organizationRepository.DeleteOrganization(organizationId);
        }

        public bool AdminExists(string identifier)
        {
            return _adminRepository.AdminExists(identifier);
        }
    }
}

