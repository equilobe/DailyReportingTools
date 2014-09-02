using AnotherJiraRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    class SprintReport
    {
        public List<Task> OldCompletedTasks { get; set; }
        public List<Task> RecentlyCompletedTasks { get; set; }
        public List<Task> InProgressTasks { get; set; }
        public List<Task> OpenTasks { get; set; }

        public AnotherJiraRestClient.Issues GetSprintTasks(Policy policy)
        {
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);
            var tasks = client.GetIssuesByJql("sprint in openSprints & project = " + policy.Project, 0, 250);
            return tasks;
        }

        public static AnotherJiraRestClient.Issues GetOldIssues(Policy policy)
        {
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);
            var issues = client.GetIssuesByJql("resolved>= '2014/09/01'", 0, 250);
            return issues;
        }
    }
}
