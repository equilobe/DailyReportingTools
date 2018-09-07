using Equilobe.DailyReport.Models.Storage;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Dashboard
{
    public class AtlassianUserWorklogs
    {
        public long AtlassianUserId { get; set; }
        public List<AtlassianWorklog> Worklogs { get; set; }
    }
}
