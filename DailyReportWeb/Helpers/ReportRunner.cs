using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Win32.TaskScheduler;
using SourceControlLogReporter.Model;
using System.Configuration;
using SourceControlLogReporter;

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

        public static void RunReportDirectly(string policyName, string draftKey)
        {
            var basePath = ConfigurationManager.AppSettings["JiraReporterPath"];
            var command = GetCommand(policyName, draftKey);
            Cmd.ExecuteSingleCommand(command, basePath);
        }

        private static string GetCommand(string policyName, string draftKey)
        {
            var draftKeyOption = " --draftKey " + draftKey;
            var command = "jiraReporter.exe" + " --policy" + @" Policies\" + policyName + ".xml" + draftKeyOption;

            return command;
        }
    }
}