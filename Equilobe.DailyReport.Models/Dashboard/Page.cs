using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Dashboard
{
    public class Page<T>
    {
        public Page()
        {
            Items = new List<T>();
        }

        public int TotalRecords { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public List<T> Items { get; set; }

        public int TotalPages
        {
            get
            {
                if (PageSize < 1 || TotalRecords < 1)
                    return 0;
                
                if ((TotalRecords % PageSize) == 0)
                    return TotalRecords / PageSize;

                return (TotalRecords / PageSize) + 1;
            }
        }

        public bool IsLastPage
        {
            get
            {
                if (TotalRecords == 0)
                    return Items.Count == 0;

                return PageIndex == TotalPages;
            }
        }
    }
}
