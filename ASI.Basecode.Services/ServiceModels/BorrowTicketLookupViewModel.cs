namespace ASI.Basecode.Services.ServiceModels
{
    public class BorrowTicketLookupViewModel
    {
        public string ReferenceCode { get; set; }
        public BorrowTicketViewModel Ticket { get; set; }
        public bool HasSearched { get; set; }
        public string Message { get; set; }
        public string OrganizationCode { get; set; }
        public bool IsSuccess => Ticket != null;
    }
}

