using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class JiraBoard
    {
        public long maxResults { get; set; }
        public long startAt { get; set; }
        public long total { get; set; }
        public bool isLast { get; set; }
        public List<Board> values { get; set; }
    }

    public class Board
    {
        public string id { get; set; }
        public string self { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }
}
