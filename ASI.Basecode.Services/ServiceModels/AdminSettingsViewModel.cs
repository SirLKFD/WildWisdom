using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class AdminSettingsViewModel
    {
        // Admin Information
        public int AdminId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        public string AdminUsername { get; set; }

        public string ProfilePicture { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        // Organization Information
        public int OrganizationId { get; set; }

        [Required(ErrorMessage = "Organization name is required")]
        [StringLength(100, ErrorMessage = "Organization name cannot exceed 100 characters")]
        public string OrganizationName { get; set; }

        [Required(ErrorMessage = "Organization type is required")]
        [StringLength(50, ErrorMessage = "Organization type cannot exceed 50 characters")]
        public string OrganizationType { get; set; }

        [StringLength(50, ErrorMessage = "Contact number cannot exceed 50 characters")]
        public string ContactNumber { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; }
    }
}

