using System.Collections.Generic;

namespace ASI.Basecode.Services.ServiceModels
{
    public class BookCheckoutViewModel
    {
        public string UserName { get; set; }
        public int DailyBorrowLimit { get; set; }
        public int BorrowedToday { get; set; }
        public int MaxActiveTickets { get; set; }
        public int ActiveTicketCount { get; set; }
        public int BorrowDaysDefault { get; set; }
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public List<BorrowTicketViewModel> PendingTickets { get; set; } = new List<BorrowTicketViewModel>();
        public List<BorrowTicketViewModel> BorrowedTickets { get; set; } = new List<BorrowTicketViewModel>();
        public List<BorrowTicketViewModel> HistoryTickets { get; set; } = new List<BorrowTicketViewModel>();

        public int RemainingDailyQuota => DailyBorrowLimit - BorrowedToday > 0 ? DailyBorrowLimit - BorrowedToday : 0;
        public int RemainingActiveQuota => MaxActiveTickets - ActiveTicketCount > 0 ? MaxActiveTickets - ActiveTicketCount : 0;
        public bool HasPolicyConfigured => DailyBorrowLimit > 0 || MaxActiveTickets > 0 || BorrowDaysDefault > 0;
    }
}

