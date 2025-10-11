using System;

namespace ASI.Basecode.Data.Models
{
    public partial class OrgBooks
    {
        public int Id { get; set; }
        public int OrganizationId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Publisher { get; set; }
        public int? YearPublished { get; set; }
        public string Edition { get; set; }
        public string Category { get; set; }
        public string Genre { get; set; }
        public string Description { get; set; }
        public int CopiesTotal { get; set; }
        public int CopiesAvailable { get; set; }
        public string CoverImage { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual Organization Organization { get; set; }
    }
}


