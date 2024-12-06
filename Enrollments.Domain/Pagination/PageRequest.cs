using System.Collections.Generic;

namespace Enrollments.Domain.Pagination
{
    public class PageRequest
    {
        public int PageOffset { get; set; }
        public int PageSize { get; set; }
        public List<Filter> Filters { get; set; }
        public string SortField { get; set; }
        public EnumSortOrder SortOrder { get; set; }

        public PageRequest()
        {
            PageOffset = 0;
            PageSize = 25;
            Filters ??= new List<Filter>();
        }

  
    }
}