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
        public static void RunProcess(string policyName, string draftKey)
        {
            var basePath = ConfigurationManager.AppSettings["JiraReporterPath"];
            var command = GetCommand(policyName, draftKey);
            Cmd.ExecuteSingleCommand(command, basePath);
        }

        private static string GetCommand(string policyName, string draftKey)
        {           
            var draftKeyOption = " --draftKey " + draftKey;
            var command = "jiraReporter.exe" + " --policy Policies\\" + policyName + ".xml" + draftKeyOption;

            return command;
        }
    }
}