namespace Equilobe.DailyReport.Models.Dashboard
{
    public class Worklog
    {
        public long TimeSpentInSeconds { get; set; }
        public string IssueKey { get; set; }
        public string IssueName { get; set; }
        public string IssueUrl { get; set; }
        public string Comment { get; set; }
        
        public string TimeSpentHumanReadable
        {
            get
            {
                var hours = TimeSpentInSeconds / 3600;
                var minutes = (TimeSpentInSeconds % 3600) / 60;

                return (minutes == 0) ? hours + "h" : hours + "h " + minutes + "m";
            }
        }
    }
}
