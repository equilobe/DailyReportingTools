using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.TaskScheduling;
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
        public IConfigurationService ConfigurationService { get; set; }

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

        public void SetTask(ScheduledTaskContext context)
        {
            using (var taskService = new TaskService())
            {
                var task = taskService.GetTask(ConfigurationService.GetTaskSchedulerFolderName() + "\\" + GetTaskKey(context.UniqueProjectKey));

                TaskDefinition taskDefinition;

                if (task == null)
                {
                    taskDefinition = taskService.NewTask();
                    taskDefinition.Actions.Add(new ExecAction(ConfigurationService.GetJiraReporterPath(),
                                                              "--uniqueProjectKey=" + context.UniqueProjectKey,
                                                              ConfigurationService.GetReportToolPath()));
                }
                else
                    taskDefinition = task.Definition;

                taskDefinition.Triggers.Clear();
                if (!string.IsNullOrEmpty(context.ReportTime))
                    taskDefinition.Triggers.Add(new DailyTrigger
                    {
                        StartBoundary = DateTime.Parse(context.ReportTime)
                    });

                GetTaskFolder(taskService).RegisterTaskDefinition(GetTaskKey(context.UniqueProjectKey), taskDefinition, TaskCreation.CreateOrUpdate, WindowsIdentity.GetCurrent().Name);
            }
        }


        public void UpdateTask(ScheduledTaskContext context)
        {
            using (var taskService = new TaskService())
            {
                var taskDefinition = taskService.GetTask(ConfigurationService.GetTaskSchedulerFolderName() + "\\" + GetTaskKey(context.UniqueProjectKey)).Definition;
                taskDefinition.Triggers.Clear();
                if (!string.IsNullOrEmpty(context.ReportTime))
                    taskDefinition.Triggers.Add(new DailyTrigger
                    {
                        StartBoundary = DateTime.Parse(context.ReportTime)
                    });

                GetTaskFolder(taskService).RegisterTaskDefinition(GetTaskKey(context.UniqueProjectKey), taskDefinition, TaskCreation.CreateOrUpdate, WindowsIdentity.GetCurrent().Name);
            }
        }

        public void DeleteMultipleTasks(ProjectListContext context)
        {
            using (var taskService = new TaskService())
            {
                var taskFolder = GetTaskFolder(taskService);
                context.UniqueProjectKeys.ForEach(uniqueProjectKey =>
                {
                    taskFolder.DeleteTask(GetTaskKey(uniqueProjectKey), false);
                });
            }
        }

        public void DeleteTask(string uniqueProjectKey)
        {
            using (var taskService = new TaskService())
            {
                var taskFolder = GetTaskFolder(taskService);
                taskFolder.DeleteTask(GetTaskKey(uniqueProjectKey), false);
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
            var taskSchedulerFolderName = ConfigurationService.GetTaskSchedulerFolderName();
            var taskFolder = taskService.RootFolder
                .SubFolders
                .Where(qr => qr.Name == taskSchedulerFolderName)
                .SingleOrDefault();

            if (taskFolder == null)
                return taskService.RootFolder.CreateFolder(taskSchedulerFolderName);

            return taskFolder;
        }
        #endregion
    }
}
