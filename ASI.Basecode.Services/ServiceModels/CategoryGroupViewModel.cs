using System.Collections.Generic;

namespace ASI.Basecode.Services.ServiceModels
{
    public class CategoryGroupViewModel
    {
        public string CategoryName { get; set; }
        public List<OrgBooksViewModel> Books { get; set; }
        public int TotalCount { get; set; }
        public string CategoryIcon { get; set; }
    }
}
