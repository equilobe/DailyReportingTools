namespace Equilobe.DailyReport.Models.Dashboard
{
    public class PagedFilter
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }

        public int Offset
        {
            get
            {
                return (PageIndex - 1) * PageSize;
            }
        }
    }
}
