using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.TaskScheduling;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Linq;
using System.Security.Principal;

namespace Equilobe.DailyReport.SL
{
    public class TaskSchedulerService : ITaskSchedulerService
    {
        public IConfigurationService ConfigurationService { get; set; }
        public IDataService DataService { get; set; }

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

        public void CreateDashboardDataSyncTask(string instanceUniqueKey)
        {
            var taskKey = GetJiraDBSyncTaskKey(instanceUniqueKey);
            var toolPath = ConfigurationService.GetPowershell();
            var arguments = GetSyncTaskArguments(instanceUniqueKey);
            var trigger = new TimeTrigger();
            trigger.Repetition.Interval = TimeSpan.FromMinutes(1);

            CreateOrUpdateTask(taskKey, toolPath, arguments, trigger);
        }

        public void SetTask(ScheduledTaskContext context)
        {
            var taskKey = GetTaskKey(context.UniqueProjectKey);
            var toolPath = ConfigurationService.GetJiraReporterPath();
            var arguments = "--uniqueProjectKey=" + context.UniqueProjectKey;
            var trigger = string.IsNullOrEmpty(context.ReportTime) ? null : new DailyTrigger
            {
                StartBoundary = DateTime.Parse(context.ReportTime)
            };

            CreateOrUpdateTask(taskKey, toolPath, arguments, trigger);
        }

        public void CreateOrUpdateTask(string taskKey, string toolPath, string arguments, Trigger trigger)
        {
            using (var ts = new TaskService())
            {
                var task = ts.GetTask(ConfigurationService.GetTaskSchedulerFolderName() + "\\" + taskKey);

                TaskDefinition taskDefinition;

                if (task == null)
                {
                    taskDefinition = ts.NewTask();
                    taskDefinition.Actions.Add(new ExecAction(toolPath, arguments, ConfigurationService.GetReportToolPath()));
                    taskDefinition.Settings.MultipleInstances = TaskInstancesPolicy.Queue;
                }
                else
                    taskDefinition = task.Definition;

                taskDefinition.Triggers.Clear();

                if (trigger != null)
                    taskDefinition.Triggers.Add(trigger);

                GetTaskFolder(ts).RegisterTaskDefinition(taskKey, taskDefinition, TaskCreation.CreateOrUpdate, WindowsIdentity.GetCurrent().Name);
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

        public void DeleteDashboardDataSyncTask(string instanceUniqueKey)
        {
            using (var taskService = new TaskService())
            {
                var taskFolder = GetTaskFolder(taskService);
                taskFolder.DeleteTask(GetJiraDBSyncTaskKey(instanceUniqueKey), false);
            }
        }

        #region Helpers
        private void RunReportTask(ProjectContext context)
        {
            var taskKey = GetTaskKey(context.UniqueProjectKey);
            using (var ts = new TaskService())
            {
                var task = ts.GetTask(ConfigurationService.GetTaskSchedulerFolderName() + "\\" + taskKey);
                task.Run();
            }
        }

        private static string GetTaskKey(string uniqueProjectKey)
        {
            return "DRT-" + uniqueProjectKey;
        }

        private string GetJiraDBSyncTaskKey(string instanceUniqueKey)
        {
            return "DD-" + instanceUniqueKey;
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

        private string GetSyncTaskArguments(string instanceUniqueKey)
        {
            var instance = DataService.GetInstanceByKey(instanceUniqueKey);
            var apiEndpoint = "\"http://localhost:59489/api/dashboardSync?instanceUniqueKey=" + instanceUniqueKey + "\"";
            var scriptPath = ConfigurationService.GetDashboardDataSyncScriptPath();

            return scriptPath + " -ApiEndpoint " + apiEndpoint;

            //return instance.BaseUrl + "report/syncDashboardData?instanceUniqueKey=" + instanceUniqueKey;
        }
        #endregion
    }
}
