using System;

namespace Equilobe.DailyReport.Models.Dashboard
{
    public class DashboardWorklog
    {
        public long TimeSpentInSeconds { get; set; }
        public string IssueKey { get; set; }
        public string IssueUrl { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
    }
}
