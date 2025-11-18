using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IOverdueReminderLogRepository
    {
        OverdueReminderLog GetByTicketId(int ticketId);
        IQueryable<OverdueReminderLog> GetLogs();
        IQueryable<OverdueReminderLog> GetLogsByTicketIds(IEnumerable<int> ticketIds);
        void Add(OverdueReminderLog log);
        void Update(OverdueReminderLog log);
        void ResolveByTicketId(int ticketId);
    }
}

