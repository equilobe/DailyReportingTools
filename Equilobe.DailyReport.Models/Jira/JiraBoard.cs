using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Jira
{
    public class JiraBoard
    {
        public long MaxResults { get; set; }
        public long StartAt { get; set; }
        public long Total { get; set; }
        public bool IsLast { get; set; }
        public List<Board> Values { get; set; }
    }

    public class Board
    {
        public string  Id { get; set; }
        public string Self { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
