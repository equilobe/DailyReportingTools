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

        public static BasicSettings GetBasicSettingsByUniqueProjectKey(this ReportsDb db, string uniqueProjectKey)
        {
            return db.BasicSettings.Single(r => r.UniqueProjectKey == uniqueProjectKey);
        }
    }
}
