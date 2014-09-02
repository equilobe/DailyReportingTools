using AnotherJiraRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class SprintReport
    {
        public List<Task> OldCompletedTasks { get; set; }
        public List<Task> RecentlyCompletedTasks { get; set; }
        public List<Task> InProgressTasks { get; set; }
        public List<Task> OpenTasks { get; set; }

        //public AnotherJiraRestClient.Issues GetSprintTasks(Policy policy)
        //{
        //    var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
        //    var client = new JiraClient(account);
        //    var tasks = client.GetIssuesByJql("sprint in openSprints & project = " + policy.Project, 0, 250);
        //    return tasks;
        //}

        private AnotherJiraRestClient.Issues GetOldIssues(Policy policy, Options options)
        {
            string toDate = Options.DateToISO(options.FromDate.AddDays(-1));
            string fromDate = Options.DateToISO(options.FromDate.AddDays(-7));
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);
            var issues = client.GetIssuesByJql("resolved >= '" + fromDate + "' & resolved <= '"+ toDate + "'", 0, 250);
            return issues;
        }

        public void GetOldCompletedTasks(Policy policy, Options options)
        {
            var OldCompletedTasks = new List<Task>();
            var issues = GetOldIssues(policy, options).issues;
            foreach (var issue in issues)
            {
                OldCompletedTasks.Add(new Task { Issue = new Issue { Key = issue.key, Summary = issue.fields.summary }, ResolutionDate = Convert.ToDateTime(issue.fields.resolutiondate) });
                OldCompletedTasks.Last().Issue.SetIssue(policy,issue);
                SetTaskDays(OldCompletedTasks.Last());
            }
            this.OldCompletedTasks = OldCompletedTasks;
        }

        private TimeSpan SetTaskTimeSpan(Task task)
        {
            var dateNow = DateTime.Now;
            var resolutionDate = task.ResolutionDate;
            TimeSpan timeAgo = dateNow - resolutionDate;
            return timeAgo;
        }

        private void SetTaskDays(Task task)
        {
            int days = SetTaskTimeSpan(task).Days;
            task.CompletedTimeAgo = string.Format("{0} days", days);
        }

        public void SortTasks()
        {
            this.OldCompletedTasks = this.OldCompletedTasks.OrderBy(date => date.ResolutionDate).ToList();
        }

    }
}
