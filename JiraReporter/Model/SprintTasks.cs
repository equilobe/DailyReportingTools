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
        public List<CompletedTasks> CompletedTasksList { get; set; } 
        public List<Issue> InProgressTasks { get; set; }
        public List<Issue> OpenTasks { get; set; }
        public List<Issue> UnassignedTasks { get; set; }
       // public int UnassignedCount { get { return UnassignedTasks.Count; } }

        public void SetSprintTasks(SvnLogReporter.Model.Policy policy, Timesheet timesheet, SvnLogReporter.Options options, List<PullRequest> pullRequests)
        {
            var issues = RestApiRequests.GetSprintTasks(policy);
            GetUnfinishedTasks(policy, issues, timesheet, pullRequests);
            var completedTasks = GetCompletedTasks(policy,options,timesheet);
            SetCompletedTasks(GroupCompletedTasks(completedTasks));
            SortTasks();
        }

        private List<Issue> GetCompletedTasks(SvnLogReporter.Model.Policy policy, SvnLogReporter.Options options, Timesheet timesheet)
        {
            var completedTasks = new List<Issue>();
            var issues = RestApiRequests.GetCompletedIssues(policy, DateTime.Today.AddDays(-7), DateTime.Now);
            foreach(var issue in issues.issues)
            {
                if(issue.fields.issuetype.subtask==false)
                    SetTasks(policy, issue, timesheet, completedTasks, null);             
            }
            completedTasks = completedTasks.OrderByDescending(d => d.ResolutionDate).ToList();
            return completedTasks; 
        }

        private void GetUnfinishedTasks(SvnLogReporter.Model.Policy policy, AnotherJiraRestClient.Issues issues, Timesheet timesheet, List<PullRequest> pullRequests)
        {
            this.InProgressTasks = new List<Issue>();
            this.OpenTasks = new List<Issue>();
            this.UnassignedTasks = new List<Issue>();
            foreach (var issue in issues.issues)
            {
                if (issue.fields.status.statusCategory.name == "In Progress")
                    SetTasks(policy, issue, timesheet, this.InProgressTasks, pullRequests);
                else
                    if (issue.fields.resolution == null)
                        SetTasks(policy, issue, timesheet, this.OpenTasks, pullRequests);
                if (issue.fields.assignee == null && issue.fields.status.statusCategory.name!="Done")
                    SetTasks(policy, issue, timesheet, this.UnassignedTasks, pullRequests);
            }
        }

        private void SetTasks(SvnLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue issue, Timesheet timesheet, List<Issue> tasks, List<PullRequest> pullRequests)
        {
            DateTime updatedDate;
            updatedDate = Convert.ToDateTime(issue.fields.updated);
            
            tasks.Add( new Issue { 
                    Key = issue.key, 
                    Summary = issue.fields.summary, 
                    RemainingEstimateSeconds = issue.fields.timeestimate, 
                    UpdatedDate = updatedDate});
            if (issue.fields.resolutiondate != null)
            {
                tasks.Last().ResolutionDate = Convert.ToDateTime(issue.fields.resolutiondate);
                tasks.Last().CompletedTimeAgo = TimeFormatting.GetStringDay(tasks.Last().ResolutionDate);
            }

            IssueAdapter.SetIssue(tasks.Last(), policy, issue, timesheet, pullRequests);
            if (tasks.Last().Subtasks != null)
            {
                IssueAdapter.SetSubtasksIssues(tasks.Last(), policy, timesheet, pullRequests);
                TasksService.HasTasksInProgress(tasks.Last());
            }
        }    

        private void SortTasks()
        {
            if (this.InProgressTasks != null)
                this.InProgressTasks = this.InProgressTasks.OrderBy(priority => priority.Priority.id).ToList();
            if (this.OpenTasks != null)
                this.OpenTasks = this.OpenTasks.OrderBy(priority => priority.Priority.id).ToList();
            if(this.UnassignedTasks!=null)
                this.UnassignedTasks = this.UnassignedTasks.OrderBy(priority => priority.Priority.id).ToList();
        }

        private IEnumerable<IGrouping<string,Issue>> GroupCompletedTasks(List<Issue> completedTasks)
        {
            var tasks = from task in completedTasks
                            group task by task.CompletedTimeAgo into newGroup
                            orderby  newGroup.Min(g=>g.ResolutionDate)
                            select newGroup;
            tasks = tasks.OrderByDescending(t => t.Min(g => g.ResolutionDate));
            return tasks;
        }

        private void SetCompletedTasks(IEnumerable<IGrouping<string,Issue>> tasks)
        {
            var completedTasksList = new List<CompletedTasks>();
            foreach(var task in tasks)
            {
                completedTasksList.Add(new CompletedTasks());
                completedTasksList.Last().Tasks = new List<Issue>();
                  foreach(var item in task)
                   {
                      completedTasksList.Last().Tasks.Add(item);
                      completedTasksList.Last().CompletedTimeAgo = item.CompletedTimeAgo;
                   }
            }
            this.CompletedTasksList = completedTasksList;            
        }

    }
}
