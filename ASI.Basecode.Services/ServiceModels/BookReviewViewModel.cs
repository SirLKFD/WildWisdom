using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Utils.DtoMapper;
using AutoMapper;
using System;

namespace ASI.Basecode.Services.ServiceModels
{
    public class BookReviewViewModel : ICustomMap
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OrgBookId { get; set; }
        public int? Rating { get; set; }
        public string ReviewText { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<BookReview, BookReviewViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.OrgBook, opt => opt.Ignore());
        }
    }
}

