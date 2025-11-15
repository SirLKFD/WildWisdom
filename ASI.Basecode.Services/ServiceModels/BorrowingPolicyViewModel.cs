using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Utils.DtoMapper;
using AutoMapper;

namespace ASI.Basecode.Services.ServiceModels
{
    public class BorrowingPolicyViewModel : ICustomMap
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public int BorrowLimitPerUser { get; set; }
        public int BorrowDaysDefault { get; set; }
        public int MaxActiveTicketsPerUser { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<BorrowingPolicy, BorrowingPolicyViewModel>().ReverseMap();
        }
    }
}

