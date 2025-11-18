using ASI.Basecode.Data.EFCore;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class OverdueReminderLogRepository : BaseRepository<OverdueReminderLog>, IOverdueReminderLogRepository
    {
        public OverdueReminderLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public OverdueReminderLog GetByTicketId(int ticketId)
        {
            return GetDbSet<OverdueReminderLog>()
                .FirstOrDefault(x => x.BorrowTicketId == ticketId);
        }

        public IQueryable<OverdueReminderLog> GetLogs()
        {
            return GetDbSet<OverdueReminderLog>();
        }

        public IQueryable<OverdueReminderLog> GetLogsByTicketIds(IEnumerable<int> ticketIds)
        {
            return GetDbSet<OverdueReminderLog>()
                .Where(x => ticketIds.Contains(x.BorrowTicketId));
        }

        public void Add(OverdueReminderLog log)
        {
            GetDbSet<OverdueReminderLog>().Add(log);
            UnitOfWork.SaveChanges();
        }

        public void Update(OverdueReminderLog log)
        {
            GetDbSet<OverdueReminderLog>().Update(log);
            UnitOfWork.SaveChanges();
        }

        public void ResolveByTicketId(int ticketId)
        {
            var log = GetByTicketId(ticketId);
            if (log == null)
            {
                return;
            }

            log.IsResolved = true;
            log.UpdatedTime = System.DateTime.Now;
            log.UpdatedBy = "SYSTEM";
            Update(log);
        }
    }
}

