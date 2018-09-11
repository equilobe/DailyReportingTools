namespace Equilobe.DailyReport.Models.Dashboard
{
    public class UserEngagement
    {
        public long CommentsCount { get; set; }
        public long CommitsCount { get; set; }
        public long LinesOfCodeAdded { get; set; }
        public long LinesOfCodeRemoved { get; set; }
        public string JiraUserKey { get; set; }
    }
}
