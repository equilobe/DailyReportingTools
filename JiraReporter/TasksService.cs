using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JiraReporter
{
    class TasksService
    {
        public static int GetTasksTimeLeftSeconds(List<Issue> tasks)
        {
            int seconds = 0;
            foreach (var task in tasks)
                if(task.SubTask==false)
                        if (task.Subtasks.Count > 0)
                            seconds += task.TotalRemainingSeconds;
                        else
                            seconds += task.RemainingEstimateSeconds;
            return seconds;
        }

        public static void HasTasksInProgress(Issue task)
        {
            if (task.Subtasks.Count > 0)
            {
                task.HasSubtasksInProgress = HasSubtasksInProgress(task);
                task.HasAssignedSubtasksInProgress = HasAssignedSubtasksInProgress(task);
            }
        }

        public static bool HasSubtasksInProgress(Issue task)
        {
            if (task.Resolution == null && task.StatusCategory.name != "In Progess" && task.SubtasksIssues.Exists(s => s.StatusCategory.name == "In Progress"))
                return true;
            return false;
        }

        public static bool HasAssignedSubtasksInProgress(Issue task)
        {
            if (HasSubtasksInProgress(task)==true && task.SubtasksIssues.Exists(s=>s.Assignee == task.Assignee))
                return true;
            return false;
        }
    }
}
