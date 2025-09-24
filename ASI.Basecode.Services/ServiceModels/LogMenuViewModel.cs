using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Utils.DtoMapper;
using AutoMapper;

namespace ASI.Basecode.Services.ServiceModels
{
    public class LogMenuViewModel : IMapFrom<Menu>, ICustomMap
    {

        public int MenuId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public bool IsDeleted { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<Menu, LogMenuViewModel>()
                .ForMember(dest => dest.MenuId, opt => opt.MapFrom(src => src.MenuID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MenuName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.MenuDescription))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted));
        }
    }
}
