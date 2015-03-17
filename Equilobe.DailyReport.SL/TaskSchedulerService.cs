using Equilobe.DailyReport.Utils;
using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;

namespace Equilobe.DailyReport.SL
{
    public class TaskSchedulerService
    {
        public string _uniqueProjectKey { get; set; }

        public TaskSchedulerService()
        {

        }

        public TaskSchedulerService(string uniqueProjectKey)
        {
            _uniqueProjectKey = uniqueProjectKey;
        }

        public bool TryRunReportTask()
        {
            try
            {
                RunReportTask();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void RunReportTask()
        {
            var key = "DRT-" + _uniqueProjectKey;
            using (var ts = new TaskService())
            {
                var task = ts.AllTasks.Single(t => t.Name == key);
                task.Run();
            }
        }

        public void Create(string reportTime)
        {
            using (var taskService = new TaskService())
            {
                TaskDefinition taskDefinition = taskService.NewTask();
                taskDefinition.Triggers.Add(new DailyTrigger
                {
                    StartBoundary = DateTime.Parse(reportTime)
                });
                taskDefinition.Actions.Add(new ExecAction(
                    ConfigurationManager.AppSettings["jiraReporterPath"],
                    "--uniqueProjectKey=" + _uniqueProjectKey,
                    ConfigurationManager.AppSettings["reportToolPath"]));

                taskDefinition.Principal.UserId = WindowsIdentity.GetCurrent().Name;
                GetTaskFolder(taskService).RegisterTaskDefinition("DRT-" + _uniqueProjectKey, taskDefinition, TaskCreation.Create, taskDefinition.Principal.UserId, ConfigurationManager.AppSettings["taskSchedulerPassword"], TaskLogonType.S4U);
            }
        }

        public void Update(string reportTime)
        {
            using (var taskService = new TaskService())
            {
                var taskDefinition = taskService.GetTask(ConfigurationManager.AppSettings["taskSchedulerFolderName"] + "\\DRT-" + _uniqueProjectKey).Definition;
                taskDefinition.Triggers.Clear();

                if (!string.IsNullOrEmpty(reportTime))
                    taskDefinition.Triggers.Add(new DailyTrigger
                    {
                        StartBoundary = DateTime.Parse(reportTime)
                    });

                taskDefinition.Principal.UserId = WindowsIdentity.GetCurrent().Name;
                GetTaskFolder(taskService).RegisterTaskDefinition("DRT-" + _uniqueProjectKey, taskDefinition, TaskCreation.Update, taskDefinition.Principal.UserId, ConfigurationManager.AppSettings["taskSchedulerPassword"], TaskLogonType.S4U);
            }
        }

        public void Delete(List<string> uniqueProjectsKey)
        {
            using (var taskService = new TaskService())
            {
                var taskFolder = GetTaskFolder(taskService);
                var tasksNames = taskFolder.AllTasks.Select(qr => qr.Name).ToList();

                uniqueProjectsKey.ForEach(uniqueProjectKey =>
                {
                    if (tasksNames.Contains("DRT-" + uniqueProjectKey))
                        taskFolder.DeleteTask("DRT-" + uniqueProjectKey);
                });
            }
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
    }
}
