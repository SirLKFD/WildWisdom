using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Utils.DtoMapper;
using AutoMapper;

namespace ASI.Basecode.Services.ServiceModels
{
    public class MenuReturnViewModel : IMapFrom<Menu>, ICustomMap
    {

        public int MenuId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public bool IsDeleted { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            // Map from Menu to MenuReturnViewModel
            configuration.CreateMap<Menu, MenuReturnViewModel>()
                .ForMember(dest => dest.MenuId, opt => opt.MapFrom(src => src.MenuID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MenuName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.MenuDescription))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted));

            // Map from MenuReturnViewModel to Menu
            configuration.CreateMap<MenuReturnViewModel, Menu>()
                .ForMember(dest => dest.MenuID, opt => opt.MapFrom(src => src.MenuId))
                .ForMember(dest => dest.MenuName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.MenuDescription, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.LogMenus, opt => opt.Ignore());
        }
    }
}
