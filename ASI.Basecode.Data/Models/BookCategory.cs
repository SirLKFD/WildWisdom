using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class BookCategory
    {
        public BookCategory()
        {
            OrgBooks = new HashSet<OrgBooks>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        public virtual ICollection<OrgBooks> OrgBooks { get; set; }
    }
}

