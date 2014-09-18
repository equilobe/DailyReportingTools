using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiraReporter.Model
{
    public class Summary
    {
        public string TotalTime { get; set; }
        public int InProgressTasksCount { get; set; }
        public string InProgressTasksTimeLeft { get; set; }
        public int InProgressTasksTimeLeftSeconds { get; set; }
        public int InProgressUnassigned { get; set; }
        public int OpenTasksCount { get; set; }
        public string OpenTasksTimeLeft { get; set; }
        public int OpenTasksTimeLeftSeconds { get; set; }
        public int OpenUnassigned { get; set; }
        public int AuthorsInvolved { get; set; }

        public Summary(List<Author> authors, SprintStatus sprint)
        {
            this.TotalTime = TimeFormatting.SetReportTotalTime(authors);
            this.SetTasksTimeLeft(authors);
            this.InProgressUnassigned = sprint.InProgressTasks.Count(tasks => tasks.Issue.SubTask == false && tasks.Issue.Label == null && tasks.Issue.Assignee == null);
            this.OpenUnassigned = sprint.OpenTasks.Count(tasks => tasks.Issue.SubTask == false && tasks.Issue.Label == null && tasks.Issue.Assignee == null);
            this.SetTasksAssignedCount(authors);
            this.AuthorsInvolved = authors.Count;
        }

        private void SetTasksTimeLeft(List<Author> authors)
        {
            this.InProgressTasksTimeLeftSeconds = 0;
            this.OpenTasksTimeLeftSeconds = 0;
            foreach (var author in authors)
            {
                this.InProgressTasksTimeLeftSeconds += TasksService.GetTasksTimeLeftSeconds(author.InProgressTasks);
                this.OpenTasksTimeLeftSeconds += TasksService.GetTasksTimeLeftSeconds(author.OpenTasks);
            }

            this.InProgressTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(this.InProgressTasksTimeLeftSeconds);
            this.OpenTasksTimeLeft = TimeFormatting.SetTimeFormat8Hour(this.OpenTasksTimeLeftSeconds);
        }

        private void SetTasksAssignedCount(List<Author> authors)
        {
            this.InProgressTasksCount = 0;
            this.OpenTasksCount = 0;
            foreach(var author in authors)
            {
                this.InProgressTasksCount += author.InProgressTasksCount;
                this.OpenTasksCount += author.OpenTasksCount;
            }
            this.InProgressTasksCount += this.InProgressUnassigned;
            this.OpenTasksCount += this.OpenUnassigned;
        }
    }
}
