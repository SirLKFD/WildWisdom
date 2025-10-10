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

        public AdminService(IAdminRepository adminRepository, IOrganizationRepository organizationRepository, IMapper mapper)
        {
            _mapper = mapper;
            _adminRepository = adminRepository;
            _organizationRepository = organizationRepository;
        }

        public LoginResult AuthenticateAdmin(string username, string password, ref Admin admin)
        {
            admin = new Admin();
            var passwordKey = PasswordManager.EncryptPassword(password);
            admin = _adminRepository.GetAdmins().Where(x => x.AdminUsername == username &&
                                                     x.AdminPassword == passwordKey).FirstOrDefault();

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
    }
}

