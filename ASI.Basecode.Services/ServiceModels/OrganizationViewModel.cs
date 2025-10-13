using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Utils.DtoMapper;
using AutoMapper;

namespace ASI.Basecode.Services.ServiceModels
{
    public class OrganizationViewModel : ICustomMap
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<Organization, OrganizationViewModel>();
            configuration.CreateMap<OrganizationViewModel, Organization>();
        }
    }
}

