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

        private AnotherJiraRestClient.Issues GetOldCompletedIssues(Policy policy, DateTime startDate, DateTime endDate)
        {
            string toDate = Options.DateToISO(endDate);
            string fromDate = Options.DateToISO(startDate);
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);
            var issues = client.GetIssuesByJql("resolved >= '" + fromDate + "' & resolved <= '"+ toDate + "'", 0, 250);
            return issues;
        }

        public void GetOldCompletedTasks(Policy policy, Options options)
        {
            var OldCompletedTasks = new List<Task>();
            var issues = GetOldCompletedIssues(policy, options.FromDate.AddDays(-6), options.FromDate.AddDays(+1)).issues;
            foreach (var issue in issues)
            {
                OldCompletedTasks.Add(new Task { Issue = new Issue { Key = issue.key, Summary = issue.fields.summary }, ResolutionDate = Convert.ToDateTime(issue.fields.resolutiondate) });
                OldCompletedTasks.Last().Issue.SetIssue(policy,issue);
                SetTaskDays(OldCompletedTasks.Last());
            }
            this.OldCompletedTasks = OldCompletedTasks;
        }

        public void GetRecentlyCompletedTasks(Policy policy, Options options, Timesheet timesheet)
        {
            var RecentlyCompletedTasks = new List<Task>();
            DateTime date;
            foreach(var issue in timesheet.Worklog.Issues)
                if(issue.Resolution!=null)
            {
                date = Convert.ToDateTime(issue.ResolutionDate);
                if (DateTime.Compare(options.FromDate, date) <= 0)
                {
                    RecentlyCompletedTasks.Add(new Task { Issue = new Issue(issue), ResolutionDate = date });
                    SetTaskHours(RecentlyCompletedTasks.Last());
                }
            }
            this.RecentlyCompletedTasks = RecentlyCompletedTasks;
        }

        public void SetSprintTasks(Policy policy)
        {
            var issues = GetSprintTasks(policy);
            GetInProgressTasks(policy, issues);
        }

        private void GetInProgressTasks(Policy policy, AnotherJiraRestClient.Issues issues)
        {
            var InProgressTasks = new List<Task>();
            DateTime date;
            foreach(var issue in issues.issues)
                if(issue.fields.status.statusCategory.name=="In Progress")
                {
                    date = Convert.ToDateTime(issue.fields.updated);
                    InProgressTasks.Add(new Task { Issue = new Issue { Key = issue.key, Summary = issue.fields.summary, ResolutionDate=null, 
                        TimeSpent = issue.fields.timespent, RemainingEstimateSeconds = issue.fields.timeestimate }, UpdatedDate = date });
                    InProgressTasks.Last().Issue.SetIssue(policy,issue);
                    InProgressTasks.Last().Issue.SetIssueTimeFormat();
                    //InProgressTasks.Last().Issue.RemainingEstimate = TimesheetService.SetTimeFormat(InProgressTasks.Last().Issue.RemainingEstimateSeconds);
                }
            this.InProgressTasks = InProgressTasks;
        }

        private AnotherJiraRestClient.Issues GetSprintTasks(Policy policy)
        {
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);
            var tasks = client.GetIssuesByJql("sprint in openSprints() & project = " + policy.Project, 0, 250);
            return tasks;
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
            int days = (int)SetTaskTimeSpan(task).TotalDays;
            task.CompletedTimeAgo = string.Format("{0} days", days);
        }

        private void SetTaskHours(Task task)
        {
            int hours = (int)SetTaskTimeSpan(task).TotalHours;
            task.CompletedTimeAgo = string.Format("{0} hours", hours);
        }

        public void SortTasks()
        {
            if(this.OldCompletedTasks!=null)
                this.OldCompletedTasks = this.OldCompletedTasks.OrderBy(date => date.ResolutionDate).ToList();
            if(this.RecentlyCompletedTasks!=null)
                this.RecentlyCompletedTasks = this.RecentlyCompletedTasks.OrderBy(date => date.ResolutionDate).ToList();
        }

    }
}
