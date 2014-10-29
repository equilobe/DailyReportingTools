using AnotherJiraRestClient;
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

        public void SetSprintTasks(SvnLogReporter.Model.Policy policy, Timesheet timesheet, SvnLogReporter.Options options, List<PullRequest> pullRequests)
        {
            var tasksService = new TasksService();
            var issues = RestApiRequests.GetSprintTasks(policy);
            var unfinishedTasks = tasksService.GetUnfinishedTasks(policy);
            tasksService.SetUnfinishedTasks(unfinishedTasks, this, timesheet, pullRequests, policy);

            var completedTasks = tasksService.GetCompletedTasks(policy, options, timesheet);
            tasksService.SetCompletedTasks(tasksService.GroupCompletedTasks(completedTasks), this);
            tasksService.SortTasks(this);
        }
    }
}
