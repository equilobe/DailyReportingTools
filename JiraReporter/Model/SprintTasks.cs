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
        public List<Task> InProgressTasks { get; set; }
        public List<Task> OpenTasks { get; set; }
        public List<Task> UnassignedTasks { get; set; }
        public int UnassignedCount { get { return UnassignedTasks.Count; } }

        public void SetSprintTasks(SvnLogReporter.Model.Policy policy, Timesheet timesheet, SvnLogReporter.Options options)
        {
            var issues = RestApiRequests.GetSprintTasks(policy);
            GetUnfinishedTasks(policy, issues, timesheet);
            var completedTasks = GetCompletedTasks(policy,options,timesheet);
            SetCompletedTasks(GroupCompletedTasks(completedTasks));
            SortTasks();
        }

        private List<Task> GetCompletedTasks(SvnLogReporter.Model.Policy policy, SvnLogReporter.Options options, Timesheet timesheet)
        {
            var completedTasks = new List<Task>();
            var issues = RestApiRequests.GetCompletedIssues(policy, DateTime.Today.AddDays(-7), DateTime.Now);
            foreach(var issue in issues.issues)
            {
                if(issue.fields.issuetype.subtask==false)
                    SetTasks(policy, issue, timesheet, completedTasks);             
            }
            completedTasks = completedTasks.OrderByDescending(d => d.ResolutionDate).ToList();
            return completedTasks; 
        }

        private void GetUnfinishedTasks(SvnLogReporter.Model.Policy policy, AnotherJiraRestClient.Issues issues, Timesheet timesheet)
        {
            this.InProgressTasks = new List<Task>();
            this.OpenTasks = new List<Task>();
            this.UnassignedTasks = new List<Task>();
            foreach (var issue in issues.issues)
            {
                if (issue.fields.status.statusCategory.name == "In Progress")
                    SetTasks(policy, issue, timesheet, this.InProgressTasks);
                else
                    if (issue.fields.resolution == null)
                        SetTasks(policy, issue, timesheet, this.OpenTasks);
                if (issue.fields.assignee == null && issue.fields.status.statusCategory.name!="Done")
                    SetTasks(policy, issue, timesheet, this.UnassignedTasks);
            }
        }

        private void SetTasks(SvnLogReporter.Model.Policy policy, AnotherJiraRestClient.Issue issue, Timesheet timesheet, List<Task> tasks)
        {
            DateTime updatedDate;
            updatedDate = Convert.ToDateTime(issue.fields.updated);
            
            tasks.Add(new Task { 
                Issue = new Issue { 
                    Key = issue.key, 
                    Summary = issue.fields.summary, 
                    RemainingEstimateSeconds = issue.fields.timeestimate }, 
               UpdatedDate = updatedDate});
            if (issue.fields.resolutiondate != null)
            {
                tasks.Last().ResolutionDate = Convert.ToDateTime(issue.fields.resolutiondate);
                tasks.Last().CompletedTimeAgo = TimeFormatting.GetStringDay(tasks.Last().ResolutionDate);
            }

            IssueAdapter.SetIssue(tasks.Last().Issue, policy, issue, timesheet);
            if (tasks.Last().Issue.Subtasks != null)
            {
                IssueAdapter.SetSubtasksIssues(tasks.Last().Issue, policy, timesheet);
                TasksService.HasTasksInProgress(tasks.Last());
            }
        }    

        private void SortTasks()
        {
            if (this.InProgressTasks != null)
                this.InProgressTasks = this.InProgressTasks.OrderBy(priority => priority.Issue.Priority.id).ToList();
            if (this.OpenTasks != null)
                this.OpenTasks = this.OpenTasks.OrderBy(priority => priority.Issue.Priority.id).ToList();
            if(this.UnassignedTasks!=null)
                this.UnassignedTasks = this.UnassignedTasks.OrderBy(priority => priority.Issue.Priority.id).ToList();
        }

        private IEnumerable<IGrouping<string,Task>> GroupCompletedTasks(List<Task> completedTasks)
        {
            var tasks = from task in completedTasks
                            group task by task.CompletedTimeAgo into newGroup
                            orderby  newGroup.Min(g=>g.ResolutionDate)
                            select newGroup;
            tasks = tasks.OrderByDescending(t => t.Min(g => g.ResolutionDate));
            return tasks;
        }

        private void SetCompletedTasks(IEnumerable<IGrouping<string,Task>> tasks)
        {
            var completedTasksList = new List<CompletedTasks>();
            foreach(var task in tasks)
            {
                completedTasksList.Add(new CompletedTasks());
                completedTasksList.Last().Tasks = new List<Task>();
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
