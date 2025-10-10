using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class ClaimTicketViewModel
    {
        [Required(ErrorMessage = "Reference code is required.")]
        public string ReferenceCode { get; set; }

        [Required(ErrorMessage = "Employee ID is required.")]
        public int EmployeeId { get; set; }

        public string Notes { get; set; }
    }
}


