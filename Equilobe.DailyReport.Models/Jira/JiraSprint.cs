using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class JiraSprints
    {
        public long maxResults { get; set; }
        public long startAt { get; set; }
        public long total { get; set; }
        public bool isLast { get; set; }
        public List<JiraSprint> values { get; set; }
    }

    public class JiraSprint
    {
        public string id { get; set; }
        public string self { get; set; }
        public string state { get; set; }
        public string name { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public DateTime completeDate { get; set; }
        public string originalBoardId { get; set; }
        public string goal { get; set; }
    }
}
