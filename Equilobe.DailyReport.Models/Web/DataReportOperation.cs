
namespace Equilobe.DailyReport.Models.Web
{
    public class DataReportOperation
    {
        public string User { get; set; }
        public string Project { get; set; }
        public SimpleResult Status { get; set; }
        public string Details { get; set; }
    }
}
