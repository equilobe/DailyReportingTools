using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Dashboard
{
    public class DashboardItem
    {
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public List<DashboardWorklogsGroup> Worklogs { get; set; }
    }
}
