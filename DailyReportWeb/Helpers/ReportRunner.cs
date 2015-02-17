using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Win32.TaskScheduler;
using System.Configuration;
using SourceControlLogReporter;
using Equilobe.DailyReport.Utils;

namespace DailyReportWeb.Helpers
{
    public class ReportRunner
    {
        public static bool TryRunReportTask(string id)
        {
            try
            {
                RunReportTask(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void RunReportTask(string id)
        {
            var key = "DRT-" + id;
            using (var ts = new TaskService())
            {
                var task = ts.AllTasks.Single(t => t.Name == key);
                task.Run();
            }
        }

        public static void RunReportDirectly(string policyName, string draftKey, bool isForced = false)
        {
            var basePath = ConfigurationManager.AppSettings["JiraReporterPath"];
            var command = GetCommand(policyName, draftKey, isForced);
            Cmd.ExecuteSingleCommand(command, basePath);
        }

        private static string GetCommand(string policyName, string draftKey, bool isForced = false)
        {
            var draftKeyOption = string.Empty;
            if (isForced)
                draftKeyOption = " --triggerKey " + draftKey;
            else
                draftKeyOption = " --draftKey " + draftKey;
            var command = "jiraReporter.exe" + " --policy" + @" Policies\" + policyName + ".xml" + draftKeyOption;

            return command;
        }
    }
}