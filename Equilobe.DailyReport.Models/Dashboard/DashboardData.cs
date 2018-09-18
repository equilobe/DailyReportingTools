using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Dashboard
{
    public class DashboardData
    {
        public bool IsAvailable { get; set; }

        public List<DashboardItem> Items { get; set; }


        public static DashboardData Unavailable()
        {
            return new DashboardData
            {
                IsAvailable = false,
                Items = null
            };
        }
    }
}
