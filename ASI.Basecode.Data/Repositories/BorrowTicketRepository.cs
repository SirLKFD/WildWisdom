using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Repositories
{
    public class BorrowTicketRepository : BaseRepository<BorrowTicket>, IBorrowTicketRepository
    {
        public BorrowTicketRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<BorrowTicket> GetTicketsByUser(int userId)
        {
            return this.GetDbSet<BorrowTicket>().Where(x => x.UserId == userId);
        }

        public IQueryable<BorrowTicket> GetTicketsByOrganization(int orgId)
        {
            return this.GetDbSet<BorrowTicket>().Where(x => x.OrganizationId == orgId);
        }

        public BorrowTicket GetTicketByReferenceCode(string code)
        {
            return this.GetDbSet<BorrowTicket>().FirstOrDefault(x => x.ReferenceCode == code);
        }

        public void AddTicket(BorrowTicket ticket)
        {
            this.GetDbSet<BorrowTicket>().Add(ticket);
            UnitOfWork.SaveChanges();
        }

        public void UpdateTicket(BorrowTicket ticket)
        {
            this.GetDbSet<BorrowTicket>().Update(ticket);
            UnitOfWork.SaveChanges();
        }

        public IQueryable<BorrowTicket> GetActiveTickets(int userId)
        {
            return this.GetDbSet<BorrowTicket>().Where(x => x.UserId == userId && 
                (x.Status == TicketStatus.NotYetClaimed || 
                 x.Status == TicketStatus.Claimed || 
                 x.Status == TicketStatus.NotYetReturned));
        }

        public IQueryable<BorrowTicket> GetExpiredTickets()
        {
            var now = DateTime.Now;
            return this.GetDbSet<BorrowTicket>().Where(x => 
                x.Status == TicketStatus.NotYetClaimed && 
                x.ExpiresAt < now);
        }

        public IQueryable<BorrowTicket> GetTicketsByStatus(int orgId, TicketStatus status)
        {
            return this.GetDbSet<BorrowTicket>().Where(x => x.OrganizationId == orgId && x.Status == status);
        }

        public BorrowTicket GetById(int id)
        {
            return this.GetDbSet<BorrowTicket>().FirstOrDefault(x => x.Id == id);
        }
    }
}


