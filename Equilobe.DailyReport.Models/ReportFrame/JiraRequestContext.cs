using Equilobe.DailyReport.Models.Interfaces;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class JiraRequestContext : IJiraRequestContext
    {
        public string BaseUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string SharedSecret { get; set; }
    }
}
