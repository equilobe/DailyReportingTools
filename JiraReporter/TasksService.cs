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

        public static void HasTasksInProgress(Task task)
        {
            if (task.Issue.Subtasks.Count > 0)
            {
                task.HasInProgress = HasInProgress(task);
                task.HasInProgressAuthor = HasInProgressAuthor(task);
            }
        }

        public static bool HasInProgress(Task task)
        {
            if (task.Issue.Resolution == null && task.Issue.StatusCategory.name != "In Progess" && task.Issue.SubtasksIssues.Exists(s => s.StatusCategory.name == "In Progress"))
                return true;
            return false;
        }

        public static bool HasInProgressAuthor(Task task)
        {
            if (HasInProgress(task)==true && task.Issue.SubtasksIssues.Exists(s=>s.Assignee == task.Issue.Assignee))
                return true;
            return false;
        }
    }
}
