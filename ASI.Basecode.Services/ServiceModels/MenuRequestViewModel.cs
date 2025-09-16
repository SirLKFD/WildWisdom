using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Utils.DtoMapper;
using AutoMapper;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class MenuRequestViewModel : IMapFrom<Menu>, ICustomMap
    {

        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; }

        public decimal Price { get; set; }

        public bool IsDeleted { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            // Map from MenuRequestViewModel to Menu
            configuration.CreateMap<MenuRequestViewModel, Menu>()
                .ForMember(dest => dest.MenuID, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.MenuName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.MenuDescription, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.LogMenus, opt => opt.Ignore());

            // Map from Menu to MenuRequestViewModel
            configuration.CreateMap<Menu, MenuRequestViewModel>()
                .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.MenuID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MenuName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.MenuDescription))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted));
        }
    }
}
