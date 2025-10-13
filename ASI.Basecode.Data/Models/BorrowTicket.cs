using System;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Data.Models
{
    public partial class BorrowTicket
    {
        // Identity & Linking
        public int Id { get; set; }
        public string ReferenceCode { get; set; }
        public int OrganizationId { get; set; }
        public int OrgBookId { get; set; }
        public int UserId { get; set; }

        // Lifecycle & Status
        public TicketStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? ClaimedAt { get; set; }
        public DateTime? CheckedOutAt { get; set; }
        public DateTime? DueAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Staff & Accountability
        public int? ClaimedByEmployeeId { get; set; }
        public int? CheckedOutByEmployeeId { get; set; }
        public int? ReturnedToEmployeeId { get; set; }
        public string Notes { get; set; }

        // Policy Snapshot
        public int BorrowDays { get; set; }
        public decimal FinePerDay { get; set; }
        public int MaxRenewals { get; set; }
        public int RenewCount { get; set; }

        // Fines & Fees
        public decimal AccruedFine { get; set; }
        public bool IsFinePaid { get; set; }

        // Audit
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        // Navigation Properties
        public virtual Organization Organization { get; set; }
        public virtual OrgBooks OrgBook { get; set; }
        public virtual User User { get; set; }
        public virtual Admin ClaimedByEmployee { get; set; }
        public virtual Admin CheckedOutByEmployee { get; set; }
        public virtual Admin ReturnedToEmployee { get; set; }
    }
}


