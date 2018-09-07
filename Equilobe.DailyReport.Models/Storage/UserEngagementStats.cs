using System;

namespace Equilobe.DailyReport.Models.Storage
{
    public class UserEngagementStats
    {
        public long Id { get; set; }
        public long AtlassianUserId { get; set; }
        public long CommentsCount { get; set; }
        public long CommitsCount { get; set; }
        public long LinesOfCodeAdded { get; set; }
        public long LinesOfCodeRemoved { get; set; }
        public DateTime Date { get; set; }

        public virtual AtlassianUser AtlassianUser { get; set; }
    }
}
