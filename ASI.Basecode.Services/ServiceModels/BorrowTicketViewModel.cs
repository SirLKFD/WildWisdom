using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Utils.DtoMapper;
using AutoMapper;
using System;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.ServiceModels
{
    public class BorrowTicketViewModel : ICustomMap
    {
        public int Id { get; set; }
        public string ReferenceCode { get; set; }
        public int OrganizationId { get; set; }
        public int OrgBookId { get; set; }
        public int UserId { get; set; }

        // Lifecycle
        public TicketStatus Status { get; set; }
        public string StatusDisplay { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? ClaimedAt { get; set; }
        public DateTime? CheckedOutAt { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Staff
        public int? ClaimedByEmployeeId { get; set; }
        public int? CheckedOutByEmployeeId { get; set; }
        public int? ReturnedToEmployeeId { get; set; }
        public string Notes { get; set; }

        // Policy Snapshot
        public int BorrowDays { get; set; }
        public decimal FinePerDay { get; set; }
        public int MaxRenewals { get; set; }
        public int RenewCount { get; set; }

        // Fines
        public decimal AccruedFine { get; set; }
        public bool IsFinePaid { get; set; }

        // Display properties
        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }
        public string UserName { get; set; }
        public string ClaimedByEmployeeName { get; set; }
        public string CheckedOutByEmployeeName { get; set; }
        public string ReturnedToEmployeeName { get; set; }

        // Calculated properties
        public bool IsExpired => Status == TicketStatus.NotYetClaimed && ExpiresAt < DateTime.Now;
        public bool IsOverdue => Status == TicketStatus.NotYetReturned && DueAt.HasValue && DueAt < DateTime.Now;
        public int DaysUntilExpiry => Status == TicketStatus.NotYetClaimed ? (int)(ExpiresAt - DateTime.Now).TotalDays : 0;
        public int DaysUntilDue => Status == TicketStatus.NotYetReturned && DueAt.HasValue ? (int)(DueAt.Value - DateTime.Now).TotalDays : 0;

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<BorrowTicket, BorrowTicketViewModel>()
                .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.BookTitle, opt => opt.Ignore())
                .ForMember(dest => dest.BookAuthor, opt => opt.Ignore())
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.ClaimedByEmployeeName, opt => opt.Ignore())
                .ForMember(dest => dest.CheckedOutByEmployeeName, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnedToEmployeeName, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.OrgBook, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.ClaimedByEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.CheckedOutByEmployee, opt => opt.Ignore())
                .ForMember(dest => dest.ReturnedToEmployee, opt => opt.Ignore());
        }
    }
}


