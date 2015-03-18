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
    }
}
