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

        private AnotherJiraRestClient.Issues GetOldCompletedIssues(Policy policy, DateTime startDate)
        {
            string fromDate = Options.DateToISO(startDate);
            string toDate = "endOfDay()";
            var account = new JiraAccount(policy.BaseUrl, policy.Username, policy.Password);
            var client = new JiraClient(account);
            var issues = client.GetIssuesByJql("resolved >= '" + fromDate + "' & resolved <= "+ toDate, 0, 250);
            return issues;
        }

        private void GetOldCompletedTasks(Policy policy, Options options, Timesheet timesheet)
        {
            var OldCompletedTasks = new List<Task>();
            var issues = GetOldCompletedIssues(policy, options.FromDate.AddDays(-6)).issues;
            foreach (var issue in issues)
            {
                OldCompletedTasks.Add(new Task { Issue = new Issue { Key = issue.key, Summary = issue.fields.summary }, ResolutionDate = Convert.ToDateTime(issue.fields.resolutiondate) });
                OldCompletedTasks.Last().Issue.SetIssue(policy, issue, timesheet);
                SetTaskCompletedTime(OldCompletedTasks.Last());
            }
            this.OldCompletedTasks = OldCompletedTasks;
        }

        private void GetRecentlyCompletedTasks(Policy policy, Options options, Timesheet timesheet)
        {
            var RecentlyCompletedTasks = new List<Task>();
            DateTime date;
            var issues = GetOldCompletedIssues(policy, options.FromDate);
            foreach(var issue in issues.issues)
                if(issue.fields.resolution!=null)
            {
                date = Convert.ToDateTime(issue.fields.resolutiondate);
                {
                    RecentlyCompletedTasks.Add(new Task { Issue = new Issue { Key = issue.key, Summary = issue.fields.summary }, ResolutionDate = date });
                    RecentlyCompletedTasks.Last().Issue.SetIssue(policy, issue, timesheet);
                    SetTaskCompletedTime(RecentlyCompletedTasks.Last());
                }
            }
            this.RecentlyCompletedTasks = RecentlyCompletedTasks;
        }

        public void SetSprintTasks(Policy policy, Timesheet timesheet, Options options)
        {
            var issues = GetSprintTasks(policy);
            GetUnfinishedTasks(policy, issues, timesheet);
            GetRecentlyCompletedTasks(policy, options, timesheet);
            GetOldCompletedTasks(policy, options, timesheet);
            SortTasks();
        }

        private void GetUnfinishedTasks(Policy policy, AnotherJiraRestClient.Issues issues, Timesheet timesheet)
        {
            var inProgressTasks = new List<Task>();
            var openTasks = new List<Task>();
            foreach (var issue in issues.issues)
                if (issue.fields.status.statusCategory.name == "In Progress")
                    GetInProgressTasks(policy, issue, timesheet, inProgressTasks);
                else
                    if (issue.fields.resolution == null)
                        GetOpenTasks(policy, issue, timesheet, openTasks);
            this.InProgressTasks = inProgressTasks;
            this.OpenTasks = openTasks;
        }

        private void GetInProgressTasks(Policy policy, AnotherJiraRestClient.Issue issue, Timesheet timesheet, List<Task> inProgressTasks)
        {
            DateTime date;
            date = Convert.ToDateTime(issue.fields.updated);
            inProgressTasks.Add(new Task { Issue = new Issue { Key = issue.key, Summary = issue.fields.summary, ResolutionDate=null, 
                TimeSpent = issue.fields.timespent, RemainingEstimateSeconds = issue.fields.timeestimate }, UpdatedDate = date});
            inProgressTasks.Last().Issue.SetIssue(policy, issue, timesheet);
            if (inProgressTasks.Last().Issue.Subtasks != null)
                inProgressTasks.Last().Issue.SetSubtasksIssues(policy, timesheet);
                         
        }

        private void GetOpenTasks(Policy policy, AnotherJiraRestClient.Issue issue, Timesheet timesheet, List<Task> openTasks)
        {
            DateTime date;
            date = Convert.ToDateTime(issue.fields.updated);
            openTasks.Add(new Task { Issue = new Issue { Key = issue.key, Summary = issue.fields.summary, ResolutionDate=null, 
                TimeSpent = issue.fields.timespent, RemainingEstimateSeconds = issue.fields.timeestimate }, UpdatedDate = date});
            openTasks.Last().Issue.SetIssue(policy, issue, timesheet);
            if (openTasks.Last().Issue.Subtasks != null)
                openTasks.Last().Issue.SetSubtasksIssues(policy, timesheet);
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

        private void SetTaskCompletedTime(Task task)
        {
            task.CompletedTimeAgo="";
            int minutes = (int)SetTaskTimeSpan(task).TotalMinutes;
            var t = TimeSpan.FromMinutes(minutes);
            var c = t.Days;
            var x = t.TotalDays;
            if (t.Days > 1)
                task.CompletedTimeAgo = string.Format("{0} days", t.Days);
            else
                if (t.Days == 1)
                    task.CompletedTimeAgo = string.Format("{0} day", t.Days);
                else
                    if (t.Hours > 1)
                        task.CompletedTimeAgo = string.Format("{0} hours", t.Hours);
                    else
                        if (t.Hours == 1)
                            task.CompletedTimeAgo = string.Format("{0} hour", t.Hours);
                        else
                            task.CompletedTimeAgo = string.Format("{0} minutes", t.Minutes);
        }

        private void SortTasks()
        {
            if(this.OldCompletedTasks!=null)
                this.OldCompletedTasks = this.OldCompletedTasks.OrderBy(date => date.ResolutionDate).ToList();
            if(this.RecentlyCompletedTasks!=null)
                this.RecentlyCompletedTasks = this.RecentlyCompletedTasks.OrderBy(date => date.ResolutionDate).ToList();
            if (this.InProgressTasks != null)
                this.InProgressTasks = this.InProgressTasks.OrderBy(priority => priority.Issue.Priority.id).ToList();
            if (this.OpenTasks != null)
                this.OpenTasks = this.OpenTasks.OrderBy(priority => priority.Issue.Priority.id).ToList();
        }

    }
}
