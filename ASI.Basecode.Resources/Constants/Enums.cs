namespace ASI.Basecode.Resources.Constants
{
    /// <summary>
    /// Class for enumerated values
    /// </summary>
    public class Enums
    {
        /// <summary>
        /// API Result Status
        /// </summary>
        public enum Status
        {
            Success,
            Error,
            CustomErr,
        }

        /// <summary>
        /// Login Result
        /// </summary>
        public enum LoginResult
        {
            Success = 0,
            Failed = 1,
        }

        /// <summary>
        /// Borrow Ticket Status
        /// </summary>
        public enum TicketStatus
        {
            NotYetClaimed = 0,
            Claimed = 1,
            NotYetReturned = 2,
            Returned = 3,
            Cancelled = 4,
            Expired = 5,
            Lost = 6
        }
    }
}
