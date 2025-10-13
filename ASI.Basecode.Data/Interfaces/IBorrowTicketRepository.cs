using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IBorrowTicketRepository
    {
        IQueryable<BorrowTicket> GetTicketsByUser(int userId);
        IQueryable<BorrowTicket> GetTicketsByOrganization(int orgId);
        BorrowTicket GetTicketByReferenceCode(string code);
        void AddTicket(BorrowTicket ticket);
        void UpdateTicket(BorrowTicket ticket);
        IQueryable<BorrowTicket> GetActiveTickets(int userId);
        IQueryable<BorrowTicket> GetExpiredTickets();
        IQueryable<BorrowTicket> GetTicketsByStatus(int orgId, TicketStatus status);
        BorrowTicket GetById(int id);
    }
}


