using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Dashboard
{
    public class DashboardPage
    {
        public bool IsAvailable { get; set; }

        public List<DashboardItem> Items { get; set; }
    }
}
