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
        public List<Issue> InProgressTasks { get; set; }
        public List<Issue> OpenTasks { get; set; }
        public List<Issue> UnassignedTasks { get; set; }
        public List<Issue> UncompletedTasks
        {
            get
            {
                return InProgressTasks.Concat(OpenTasks).ToList();
            }
        }
        public int UnassignedTasksErrorCount { get; set; }
        public int CompletedTasksErrorCount { get; set; }

        public void SetSprintTasks(SourceControlLogReporter.Model.Policy policy, SourceControlLogReporter.Options options, List<PullRequest> pullRequests)
        {
            var tasksService = new TasksService();
            var issues = RestApiRequests.GetSprintTasks(policy);
            var unfinishedTasks = tasksService.GetUnfinishedTasks(policy);
            tasksService.SetUnfinishedTasks(unfinishedTasks, this, pullRequests, policy);

            var completedTasks = tasksService.GetCompletedTasks(policy, options);
            tasksService.SetCompletedTasks(tasksService.GroupCompletedTasks(completedTasks), this);
            tasksService.SortTasks(this);
            SetSprintTasksErrors(policy);
        }

        private void SetSprintTasksErrors(SourceControlLogReporter.Model.Policy policy)
        {
            int completedErrors = 0;
            TasksService.SetErrors(UnassignedTasks, policy);
            foreach (var list in CompletedTasks)
            {
                TasksService.SetErrors(list.Value, policy);
                completedErrors += TasksService.GetErrorsCount(list.Value);
            }
            CompletedTasksErrorCount = completedErrors;
            UnassignedTasksErrorCount = TasksService.GetErrorsCount(UnassignedTasks);
        }   
    }
}
