using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _repository;
        private readonly IMapper _mapper;

        public OrganizationService(IOrganizationRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public List<OrganizationViewModel> GetAllOrganizations()
        {
            var organizations = _repository.GetOrganizations().ToList();
            return _mapper.Map<List<OrganizationViewModel>>(organizations);
        }

        public bool ValidateOrganization(string name)
        {
            return !_repository.OrganizationExists(name);
        }

        public void AddOrganization(OrganizationViewModel model)
        {
            var organization = new Organization();
            _mapper.Map(model, organization);
            organization.CreatedTime = DateTime.Now;
            organization.UpdatedTime = DateTime.Now;
            organization.CreatedBy = System.Environment.UserName;
            organization.UpdatedBy = System.Environment.UserName;

            _repository.AddOrganization(organization);
        }

        public OrganizationViewModel GetOrganizationById(int id)
        {
            var organization = _repository.GetOrganizationById(id);
            return _mapper.Map<OrganizationViewModel>(organization);
        }
    }
}

