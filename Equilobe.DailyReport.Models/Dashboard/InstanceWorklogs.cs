using Equilobe.DailyReport.Models.Storage;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Dashboard
{
    public class InstanceWorklogs
    {
        public long Id { get; set; }
        public List<AtlassianWorklog> Worklogs { get; set; }
    }
}
