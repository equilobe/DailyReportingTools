using Equilobe.DailyReport.Utils;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class TaskSchedulerService
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

        public static void Create(string uniqueProjectKey, string reportTime)
        {
            using (TaskService taskService = new TaskService())
            {
                TaskDefinition taskDefinition = taskService.NewTask();
                taskDefinition.Triggers.Add(new DailyTrigger
                {
                    StartBoundary = DateTime.Parse(reportTime)
                });
                taskDefinition.Actions.Add(new ExecAction(
                    ConfigurationManager.AppSettings["jiraReporterPath"],
                    "uniqueProjectKey=" + uniqueProjectKey,
                    ConfigurationManager.AppSettings["reportToolPath"]));

                GetTaskFolder().RegisterTaskDefinition("DRT-" + uniqueProjectKey, taskDefinition);
            }
        }

        public static void Update(string uniqueProjectKey, string reportTime)
        {
            using (TaskService taskService = new TaskService())
            {
                var taskDefinition = taskService.GetTask(ConfigurationManager.AppSettings["taskSchedulerFolderName"] + "\\DRT-" + uniqueProjectKey).Definition;
                taskDefinition.Triggers.Clear();

                if (!string.IsNullOrEmpty(reportTime))
                    taskDefinition.Triggers.Add(new DailyTrigger
                    {
                        StartBoundary = DateTime.Parse(reportTime)
                    });

                GetTaskFolder().RegisterTaskDefinition("DRT-" + uniqueProjectKey, taskDefinition);
            }
        }

        public static void Delete(List<string> uniqueProjectsKey)
        {
            var taskFolder = GetTaskFolder();
            uniqueProjectsKey.ForEach(uniqueProjectKey =>
                taskFolder.DeleteTask("DRT-" + uniqueProjectKey));
        }

        private static TaskFolder GetTaskFolder()
        {
            using (TaskService taskService = new TaskService())
            {
                var taskFolder = taskService.RootFolder
                    .SubFolders
                    .Where(qr => qr.Name == ConfigurationManager.AppSettings["taskSchedulerFolderName"])
                    .SingleOrDefault();

                if (taskFolder == null)
                    return taskService.RootFolder.CreateFolder(ConfigurationManager.AppSettings["taskSchedulerFolderName"]);

                return taskFolder;
            }
        }
    }
}
