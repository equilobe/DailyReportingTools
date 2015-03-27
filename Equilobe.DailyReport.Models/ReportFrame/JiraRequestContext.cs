using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Storage;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class JiraRequestContext : IJiraRequestContext
    {
        public string BaseUrl { get; set; }

        public string JiraUsername { get; set; }

        public string JiraPassword { get; set; }

        public string SharedSecret { get; set; }

        public JiraRequestContext()
        {
        }

        public JiraRequestContext(InstalledInstance instance)
        {
            BaseUrl = instance.BaseUrl;
            JiraUsername = instance.JiraUsername;
            JiraPassword = instance.JiraPassword;
            SharedSecret = instance.SharedSecret;
        }
    }
}
