using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Utils.DtoMapper;
using AutoMapper;
using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class OrgBooksViewModel : ICustomMap
    {
        public int Id { get; set; }

        public int OrganizationId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(200)]
        public string Author { get; set; }

        [StringLength(20)]
        public string ISBN { get; set; }

        [StringLength(200)]
        public string Publisher { get; set; }

        public int? YearPublished { get; set; }

        [StringLength(50)]
        public string Edition { get; set; }

        [StringLength(100)]
        public string Category { get; set; }

        [StringLength(100)]
        public string Genre { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Total copies is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Total copies must be at least 1.")]
        public int CopiesTotal { get; set; }

        public int CopiesAvailable { get; set; }

        [StringLength(500)]
        public string CoverImage { get; set; }

        public bool IsActive { get; set; }

        // Additional display properties
        public string OrganizationName { get; set; }
        public decimal? AverageRating { get; set; }
        public int? UserRating { get; set; }
        public string UserReviewText { get; set; }
        public int BorrowCount { get; set; }
        
        // Admin/Organization contact details
        public string AdminName { get; set; }
        public string AdminProfilePicture { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<OrgBooks, OrgBooksViewModel>()
                .ForMember(dest => dest.OrganizationName, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Organization, opt => opt.Ignore());
        }
    }
}


