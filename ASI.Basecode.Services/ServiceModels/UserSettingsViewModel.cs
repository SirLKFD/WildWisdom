using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Utils.DtoMapper;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class UserSettingsViewModel : ICustomMap
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [Display(Name = "Username")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Display(Name = "Organization")]
        public int? OrganizationId { get; set; }

        [Display(Name = "Organization Name")]
        public string OrganizationName { get; set; }

        [Display(Name = "Organization Type")]
        public string OrganizationType { get; set; }

        [Display(Name = "Profile Picture")]
        public string ProfilePicture { get; set; }

        [Display(Name = "Upload New Profile Picture")]
        public IFormFile ProfilePictureFile { get; set; }

        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Account Created")]
        public DateTime CreatedTime { get; set; }

        [Display(Name = "Last Updated")]
        public DateTime UpdatedTime { get; set; }

        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string UpdatedBy { get; set; }

        // Additional properties for tabs
        public List<BorrowTicketViewModel> BorrowedBooks { get; set; } = new List<BorrowTicketViewModel>();
        public List<BorrowTicketViewModel> OverdueBooks { get; set; } = new List<BorrowTicketViewModel>();
        public List<BorrowTicketViewModel> HistoryBooks { get; set; } = new List<BorrowTicketViewModel>();
        public List<BorrowTicketViewModel> PendingTickets { get; set; } = new List<BorrowTicketViewModel>();
        public List<WishlistItemViewModel> WishlistItems { get; set; } = new List<WishlistItemViewModel>();
        public Dictionary<int, OverdueReminderInfoViewModel> OverdueReminderStatuses { get; set; } = new();

        [Display(Name = "Send me overdue reminder emails")]
        public bool ReceiveOverdueEmails { get; set; } = true;

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<User, UserSettingsViewModel>()
                .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : ""))
                .ForMember(dest => dest.OrganizationType, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Type : ""))
                .ForMember(dest => dest.ProfilePictureFile, opt => opt.Ignore())
                .ForMember(dest => dest.OldPassword, opt => opt.Ignore())
                .ForMember(dest => dest.NewPassword, opt => opt.Ignore())
                .ForMember(dest => dest.ConfirmPassword, opt => opt.Ignore());

            configuration.CreateMap<UserSettingsViewModel, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedTime, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore());
        }
    }
}

