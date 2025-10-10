using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IBorrowTicketService
    {
        BorrowTicketViewModel CreateBorrowRequest(int userId, int bookId);
        string GenerateReferenceCode(int orgId);
        void ClaimTicket(string referenceCode, int employeeId, string notes);
        void ReturnBook(string referenceCode, int employeeId);
        void CancelTicket(int ticketId, string reason);
        void ExpireUnclaimedTickets();
        decimal CalculateFine(BorrowTicketViewModel ticket);
        List<BorrowTicketViewModel> GetUserTickets(int userId, TicketStatus? status = null);
        List<BorrowTicketViewModel> GetOrganizationTickets(int orgId, TicketStatus? status = null);
        BorrowTicketViewModel GetTicketByReferenceCode(string referenceCode);
        BorrowTicketViewModel GetTicketById(int id);
        bool ValidateBorrowEligibility(int userId, int orgId);
        void RenewTicket(int ticketId);
        void PayFine(int ticketId);
    }
}


