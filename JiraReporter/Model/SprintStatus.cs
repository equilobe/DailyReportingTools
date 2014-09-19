using AnotherJiraRestClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class SprintStatus
    {
        public List<CompletedTasks> CompletedTasksList { get; set; } 
        public List<Task> InProgressTasks { get; set; }
        public List<Task> OpenTasks { get; set; }
        public List<Task> UnassignedTasks { get; set; }
        public int UnassignedCount { get { return UnassignedTasks.Count(tasks => tasks.Issue.SubTask == false && tasks.Issue.Label == null); } }

        public void SetSprintTasks(Policy policy, Timesheet timesheet, Options options)
        {
            var issues = RestApiRequests.GetSprintTasks(policy);
            GetUnfinishedTasks(policy, issues, timesheet);
            var completedTasks = GetCompletedTasks(policy,options,timesheet);
            SetCompletedTasks(GetCompletedTasks(completedTasks));
            SortTasks();
        }

        private List<Task> GetCompletedTasks(Policy policy, Options options, Timesheet timesheet)
        {
            var completedTasks = new List<Task>();
            var issues = RestApiRequests.GetCompletedIssues(policy, options.FromDate.AddDays(-6), DateTime.Now);
            foreach(var issue in issues.issues)
            {
                if(issue.fields.issuetype.subtask==false)
                    SetTasks(policy, issue, timesheet, completedTasks);             
            }
            completedTasks = completedTasks.OrderByDescending(d => d.ResolutionDate).ToList();
            return completedTasks; 
        }

        private void GetUnfinishedTasks(Policy policy, AnotherJiraRestClient.Issues issues, Timesheet timesheet)
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
                if (issue.fields.assignee == null && issue.fields.resolution==null)
                    SetTasks(policy, issue, timesheet, this.UnassignedTasks);
            }
        }

        private void SetTasks(Policy policy, AnotherJiraRestClient.Issue issue, Timesheet timesheet, List<Task> tasks)
        {
            DateTime updatedDate;
            updatedDate = Convert.ToDateTime(issue.fields.updated);
            
            tasks.Add(new Task { Issue = new Issue { Key = issue.key, Summary = issue.fields.summary, TimeSpent = issue.fields.timespent, 
                RemainingEstimateSeconds = issue.fields.timeestimate }, UpdatedDate = updatedDate});
            if (issue.fields.resolutiondate != null)
            {
                tasks.Last().ResolutionDate = Convert.ToDateTime(issue.fields.resolutiondate);
                tasks.Last().CompletedTimeAgo = TimeFormatting.GetCompletedTime(tasks.Last().ResolutionDate);
            }

            tasks.Last().Issue.SetIssue(policy, issue, timesheet);
            if (tasks.Last().Issue.Subtasks != null)
            {
                tasks.Last().Issue.SetSubtasksIssues(policy, timesheet);
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

        private IEnumerable<IGrouping<int,Task>> GetCompletedTasks(List<Task> completedTasks)
        {
            var tasks = from task in completedTasks
                            group task by task.ResolutionDate.Day into newGroup
                            orderby newGroup.Key
                            select newGroup;
            tasks = tasks.OrderByDescending(d => d.Key);
            return tasks;
        }

        private void SetCompletedTasks(IEnumerable<IGrouping<int,Task>> tasks)
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
