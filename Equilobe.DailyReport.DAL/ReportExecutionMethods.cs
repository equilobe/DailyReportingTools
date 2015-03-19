using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.DAL
{
    public static class ReportExecutionMethods
    {
        public static void SetExecutionInstanceDate(this ReportsDb db, long id)
        {
            var reportExecutionInstance = db.ReportExecutionInstances.Single(e => e.Id == id);
            reportExecutionInstance.DateExecuted = DateTime.Now;
        }

        public static ReportSettings GetReportSettingsByUniqueProjectKey(this ReportsDb db, string uniqueProjectKey)
        {
            return db.ReportSettings.Single(r => r.UniqueProjectKey == uniqueProjectKey);
        }
    }
}
