﻿using AnotherJiraRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class SprintTasks
    {
        public Dictionary<string, List<Issue>> CompletedTasks { get; set; }
        public UncompletedTasks InProgressTasks { get; set; }
        public UncompletedTasks OpenTasks { get; set; }
        public UncompletedTasks UnassignedTasks { get; set; }
        public List<Issue> UncompletedTasks { get; set; }
        public int UnassignedTasksErrorCount { get; set; }
        public int CompletedTasksErrorCount { get; set; }

        public void SetSprintTasks(SourceControlLogReporter.Model.Policy policy, Timesheet timesheet, SourceControlLogReporter.Options options, List<PullRequest> pullRequests)
        {
            var tasksService = new TasksService();
            var issues = RestApiRequests.GetSprintTasks(policy);
            var unfinishedTasks = tasksService.GetUnfinishedTasks(policy);
            tasksService.SetUnfinishedTasks(unfinishedTasks, this, timesheet, pullRequests, policy);

            var completedTasks = tasksService.GetCompletedTasks(policy, options, timesheet);
            tasksService.SetCompletedTasks(tasksService.GroupCompletedTasks(completedTasks), this);
            tasksService.SortTasks(this);
            SetSprintTasksErrors();
        }

        private void SetSprintTasksErrors()
        {
            CompletedTasksErrorCount = CompletedTasks.Values.Sum(i => i.Sum(er => er.ErrorsCount));
            UnassignedTasksErrorCount = UnassignedTasks.Issues.Sum(t => t.ErrorsCount);
        }   
    }
}
