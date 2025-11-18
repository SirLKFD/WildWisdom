using System;

namespace ASI.Basecode.Services.ServiceModels
{
    public class OverdueReminderInfoViewModel
    {
        public DateTime? LastSentAt { get; set; }
        public DateTime? NextSendAt { get; set; }
        public int ReminderCount { get; set; }
        public decimal LastFineAmount { get; set; }
        public bool IsResolved { get; set; }
    }
}

