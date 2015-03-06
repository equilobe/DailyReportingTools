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
            using (TaskService taskService = new TaskService())
            {
                TaskDefinition taskDefinition = taskService.NewTask();
                taskDefinition.Triggers.Add(new DailyTrigger
                {
                    StartBoundary = DateTime.Parse(reportTime)
                });
                taskDefinition.Actions.Add(new ExecAction(
                    ConfigurationManager.AppSettings["jiraReporterPath"],
                    "uniqueProjectKey=" + _uniqueProjectKey,
                    ConfigurationManager.AppSettings["reportToolPath"]));

                GetTaskFolder().RegisterTaskDefinition("DRT-" + _uniqueProjectKey, taskDefinition);
            }
        }

        public void Update(string reportTime)
        {
            using (TaskService taskService = new TaskService())
            {
                var taskDefinition = taskService.GetTask(ConfigurationManager.AppSettings["taskSchedulerFolderName"] + "\\DRT-" + _uniqueProjectKey).Definition;
                taskDefinition.Triggers.Clear();

                if (!string.IsNullOrEmpty(reportTime))
                    taskDefinition.Triggers.Add(new DailyTrigger
                    {
                        StartBoundary = DateTime.Parse(reportTime)
                    });

                GetTaskFolder().RegisterTaskDefinition("DRT-" + _uniqueProjectKey, taskDefinition);
            }
        }

        public void Delete(List<string> uniqueProjectsKey)
        {
            var taskFolder = GetTaskFolder();
            uniqueProjectsKey.ForEach(uniqueProjectKey =>
                taskFolder.DeleteTask("DRT-" + uniqueProjectKey));
        }

        private TaskFolder GetTaskFolder()
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
