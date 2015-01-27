using SourceControlLogReporter;
using SourceControlLogReporter.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DailyReportWeb.Helpers
{
    public class CmdProcess
    {
        public static void RunProcess(string policyPath, string draftKey)
        {
            var command = GetCommand(policyPath, draftKey);
            Cmd.ExecuteSingleCommand(command);
        }

        private static string GetCommand(string policyPath, string draftKey)
        {
            var basePath = ConfigurationManager.AppSettings["JiraReporterPath"];
            var exePath = basePath + "\\jiraReporter.exe";
            var draftKeyOption = " --draftKey=" + draftKey;
            var command = "start " + exePath + " --policy=" + policyPath + draftKeyOption;

            return command;
        }
    }
}