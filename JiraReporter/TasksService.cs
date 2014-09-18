using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraReporter
{
    class TasksService
    {
        public static int GetTasksTimeLeftSeconds(List<Task> tasks)
        {
            int seconds = 0;
            foreach (var task in tasks)
                if(task.Issue.SubTask==false)
                        if (task.Issue.Subtasks.Count > 0)
                            seconds += task.Issue.TotalRemainingSeconds;
                        else
                            seconds += task.Issue.RemainingEstimateSeconds;
            return seconds;
        }
    }
}
