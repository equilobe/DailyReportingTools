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
            this.InProgressTasksCount = sprint.InProgressTasks.Count(tasks => tasks.Issue.SubTask == false && tasks.Issue.Label == null);
            this.OpenTasksCount = sprint.OpenTasks.Count(tasks => tasks.Issue.SubTask == false && tasks.Issue.Label == null);
            this.GetTasksTimeLeft(authors);
            this.InProgressUnassigned = sprint.InProgressTasks.Count(tasks => tasks.Issue.SubTask == false && tasks.Issue.Label == null && tasks.Issue.Assignee == null);
            this.OpenUnassigned = sprint.OpenTasks.Count(tasks => tasks.Issue.SubTask == false && tasks.Issue.Label == null && tasks.Issue.Assignee == null);
            this.AuthorsInvolved = authors.Count;
        }

        private void GetTasksTimeLeft(List<Author> authors)
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

        private static string GetOpenTasksTimeLeft(List<Author> authors)
        {
            int seconds = 0;
            foreach (var author in authors)
                seconds += TasksService.GetTasksTimeLeftSeconds(author.OpenTasks);
            return TimeFormatting.SetTimeFormat8Hour(seconds);
        }
    }
}
