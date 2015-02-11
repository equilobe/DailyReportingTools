using Equilobe.DailyReport.Models.Jira;
using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.ReportPolicy;

namespace JiraReporter.Services
{
    class DayLogLoader
    {
        public static JiraDayLog CreateDayLog(JiraAuthor author, DateTime date, JiraReport context)
        {
            var options = context.Options;
            var dayLog = new JiraDayLog();

            dayLog.Commits = AuthorHelpers.GetDayLogCommits(author, date);
            dayLog.Date = date;
            dayLog.Title = TimeFormatting.GetStringDay(date, context.ReportDate);

            if (author.Issues != null)
                if (author.Issues.Count > 0)
                {
                    dayLog.Issues = new List<Issue>();
                    foreach (var issue in author.Issues)
                    {
                        dayLog.Issues.Add(new Issue(issue));
                        IssueAdapter.RemoveWrongEntries(dayLog.Issues.Last(), date);
                        IssueAdapter.TimeSpentFromEntries(dayLog.Issues.Last());
                        IssueAdapter.SetTimeFormat(dayLog.Issues.Last());
                        dayLog.TimeSpent += dayLog.Issues.Last().TimeSpent;
                    }
                }
            IssueAdapter.AdjustIssueCommits(dayLog);
            IssueAdapter.RemoveWrongIssues(dayLog.Issues);

            if (dayLog.Issues != null)
                dayLog.Issues = TasksService.GetParentTasks(dayLog.Issues, author);
            dayLog.UnsyncedCommits = new List<JiraCommit>(dayLog.Commits.FindAll(c => c.TaskSynced == false));
            dayLog.TimeLogged = dayLog.TimeSpent.SetTimeFormat();

            return dayLog;
        }
    }
}
