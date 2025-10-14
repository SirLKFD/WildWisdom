using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Organization
    {
        public Organization()
        {
            Users = new HashSet<User>();
            Admins = new HashSet<Admin>();
            OrgBooks = new HashSet<OrgBooks>();
            BorrowTickets = new HashSet<BorrowTicket>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Admin> Admins { get; set; }
        public virtual ICollection<OrgBooks> OrgBooks { get; set; }
        public virtual ICollection<BorrowTicket> BorrowTickets { get; set; }
        public virtual BorrowingPolicy BorrowingPolicy { get; set; }
        public virtual FinePolicy FinePolicy { get; set; }
        public virtual RenewalPolicy RenewalPolicy { get; set; }
        public virtual ClaimPolicy ClaimPolicy { get; set; }
    }
}

