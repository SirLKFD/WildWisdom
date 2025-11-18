using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IOverdueReminderService
    {
        Task<int> SendOverdueRemindersAsync(CancellationToken cancellationToken = default);
        Dictionary<int, OverdueReminderInfoViewModel> GetReminderStatuses(IEnumerable<int> ticketIds);
    }
}

