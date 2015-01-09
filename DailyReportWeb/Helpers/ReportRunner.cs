using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Win32.TaskScheduler;

namespace DailyReportWeb.Helpers
{
    public class ReportRunner
    {
        public static bool TryRunReport(string id)
        {
            try
            {
                RunReport(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void RunReport(string id)
        {
            var key = "DRT-" + id;
            using (var ts = new TaskService())
            {
                var task = ts.AllTasks.Single(t => t.Name == key);
                task.Run();
            }
        }
    }
}