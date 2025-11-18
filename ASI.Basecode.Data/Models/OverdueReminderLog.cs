using System;

namespace ASI.Basecode.Data.Models
{
    public class OverdueReminderLog
    {
        public int Id { get; set; }
        public int BorrowTicketId { get; set; }
        public int UserId { get; set; }
        public DateTime? LastSentAt { get; set; }
        public DateTime NextSendAt { get; set; }
        public int ReminderCount { get; set; }
        public decimal LastFineAmount { get; set; }
        public bool IsResolved { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime UpdatedTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        public virtual BorrowTicket BorrowTicket { get; set; }
        public virtual User User { get; set; }
    }
}

