using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.TaskScheduling;
using Equilobe.DailyReport.Utils;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;

namespace Equilobe.DailyReport.SL
{
    public class TaskSchedulerService : ITaskSchedulerService
    {
        public bool TryRunReportTask(ProjectContext context)
        {
            try
            {
                RunReportTask(context);
                return true;
            }
            catch
            {
                return false;
            }
        }        

        public void CreateTask(ScheduledTaskContext context)
        {
            using (var taskService = new TaskService())
            {
                TaskDefinition taskDefinition = taskService.NewTask();
                taskDefinition.Triggers.Add(new DailyTrigger
                {
                    StartBoundary = DateTime.Parse(context.ReportTime)
                });
                taskDefinition.Actions.Add(new ExecAction(
                    ConfigurationManager.AppSettings["jiraReporterPath"],
                    "--uniqueProjectKey=" + context.UniqueProjectKey,
                    ConfigurationManager.AppSettings["reportToolPath"]));

                GetTaskFolder(taskService).RegisterTaskDefinition(GetTaskKey(context.UniqueProjectKey), taskDefinition, TaskCreation.Create, WindowsIdentity.GetCurrent().Name);
            }
        }

        public void UpdateTask(ScheduledTaskContext context)
        {
            using (var taskService = new TaskService())
            {
                var taskDefinition = taskService.GetTask(ConfigurationManager.AppSettings["taskSchedulerFolderName"] + "\\"+ GetTaskKey(context.UniqueProjectKey)).Definition;
                taskDefinition.Triggers.Clear();

                if (!string.IsNullOrEmpty(context.ReportTime))
                    taskDefinition.Triggers.Add(new DailyTrigger
                    {
                        StartBoundary = DateTime.Parse(context.ReportTime)
                    });

                GetTaskFolder(taskService).RegisterTaskDefinition(GetTaskKey(context.UniqueProjectKey), taskDefinition, TaskCreation.Update, WindowsIdentity.GetCurrent().Name);
            }
        }

        public void DeleteMultipleTasks(ProjectListContext context)
        {
            using (var taskService = new TaskService())
            {
                var taskFolder = GetTaskFolder(taskService);
                var tasksNames = taskFolder.AllTasks.Select(qr => qr.Name).ToList();

                context.UniqueProjectKeys.ForEach(uniqueProjectKey =>
                {
                    if (tasksNames.Contains(GetTaskKey(uniqueProjectKey)))
                        taskFolder.DeleteTask(GetTaskKey(uniqueProjectKey));
                });
            }
        }

        #region Helpers
        private void RunReportTask(ProjectContext context)
        {
            var taskKey = GetTaskKey(context.UniqueProjectKey);
            using (var ts = new TaskService())
            {
                var task = ts.AllTasks.Single(t => t.Name == taskKey);
                task.Run();
            }
        }

        private static string GetTaskKey(string uniqueProjectKey)
        {
            return "DRT-" + uniqueProjectKey;
        }

        private TaskFolder GetTaskFolder(TaskService taskService)
        {
            var taskFolder = taskService.RootFolder
                .SubFolders
                .Where(qr => qr.Name == ConfigurationManager.AppSettings["taskSchedulerFolderName"])
                .SingleOrDefault();

            if (taskFolder == null)
                return taskService.RootFolder.CreateFolder(ConfigurationManager.AppSettings["taskSchedulerFolderName"]);

            return taskFolder;
        }
        #endregion
    }
}
