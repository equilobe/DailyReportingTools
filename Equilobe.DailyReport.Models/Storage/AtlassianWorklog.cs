using System;

namespace Equilobe.DailyReport.Models.Storage
{
    public class AtlassianWorklog
    {
        public long Id { get; set; }
        public long InstalledInstanceId { get; set; }
        public long AtlassianUserId { get; set; }
        public long IssueId { get; set; }
        public string IssueKey { get; set; }
        public long JiraWorklogId { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime StartedAt { get; set; }
        public long TimeSpentInSeconds { get; set; }
        public DateTime LastSync { get; set; }

        public virtual AtlassianUser AtlassianUser { get; set; }
    }
}
