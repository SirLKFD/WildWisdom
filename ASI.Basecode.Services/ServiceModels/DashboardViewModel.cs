namespace ASI.Basecode.Services.ServiceModels
{
    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int BorrowedToday { get; set; }
        public int PendingClaims { get; set; }
        public int ActiveBorrows { get; set; }
        public int OverdueCount { get; set; }
    }
}
