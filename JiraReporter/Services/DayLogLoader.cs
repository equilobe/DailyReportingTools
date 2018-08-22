using Equilobe.DailyReport.Models.ReportFrame;
using JiraReporter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Utils;
using JiraReporter.Helpers;

namespace JiraReporter.Services
{
    class DayLogLoader
    {
        public JiraAuthor _author;
        public DateTime _date;
        public JiraReport _context;

        public DayLogLoader(JiraAuthor author, DateTime date, JiraReport context)
        {
            _author = author;
            _date = date;
            _context = context;
        }

        public JiraDayLog CreateDayLog()
        {
            var dayLog = new JiraDayLog();

            dayLog.Commits = AuthorHelpers.GetDayLogCommits(_author, _date, _context.OffsetFromUtc);
            dayLog.Date = _date;
            dayLog.Title = GetDaylogTitle(_date, _context);
            dayLog.AuthorName = _author.Name;

            if (_author.Issues != null)
                if (_author.Issues.Count > 0)
                {
                    dayLog.Issues = new List<IssueDetailed>();
                    foreach (var issue in _author.Issues)
                        AddIssueToDaylog(dayLog, issue);
                }
            IssueAdapter.AdjustIssueCommits(dayLog, _context.OffsetFromUtc);
            IssueAdapter.RemoveWrongIssues(dayLog.Issues);

            if (dayLog.Issues != null)
            {
                dayLog.Issues = TaskLoader.GetParentTasks(dayLog.Issues, _author);
                dayLog.Issues.ForEach(SetHasWorkLoggedProperty);
            }

            dayLog.UnsyncedCommits = new List<JiraCommit>(dayLog.Commits.FindAll(c => c.TaskSynced == false));
            dayLog.TimeLogged = dayLog.TimeSpent.SetTimeFormat();

            return dayLog;
        }

        static string GetDaylogTitle(DateTime date, JiraReport context)
        {
            var title = date.DayOfWeek.ToString();

            if ((context.ToDate - context.FromDate).Days > 7)
                title += " (" + date.ToString("dd MMM") + ")";

            return title;
        }

        private void AddIssueToDaylog(JiraDayLog dayLog, IssueDetailed issue)
        {
            var currentIssue = new IssueDetailed();
            issue.CopyTo<IssueDetailed>(currentIssue);
            currentIssue.ExistsInTimesheet = true;

            currentIssue.Entries = issue.Entries
                .Where(e => e.AuthorFullName == _author.Name)
                .Where(e => e.StartedAt.ToOriginalTimeZone(_context.OffsetFromUtc).Date >= _date)
                .Where(e => e.StartedAt.ToOriginalTimeZone(_context.OffsetFromUtc) < _date.AddDays(1))
                .ToList(); 

            IssueAdapter.TimeSpentFromEntries(currentIssue);
            IssueAdapter.SetTimeFormat(currentIssue);
            SetSubtaskEntries(currentIssue);
            dayLog.Issues.Add(currentIssue);
            dayLog.TimeSpent += currentIssue.TimeSpent;
        }

        private void SetSubtaskEntries(IssueDetailed issue)
        {
            if (issue.SubtasksDetailed.IsEmpty())
                return;

            foreach(var subtask in issue.SubtasksDetailed)
            {
                subtask.Entries = subtask.Entries
                    .Where(e => e.AuthorFullName == _author.Name)
                    .Where(e => e.StartedAt.ToOriginalTimeZone(_context.OffsetFromUtc).Date >= _date)
                    .Where(e => e.StartedAt.ToOriginalTimeZone(_context.OffsetFromUtc) < _date.AddDays(1))
                    .ToList();

                IssueAdapter.TimeSpentFromEntries(subtask);
                IssueAdapter.SetTimeFormat(subtask);
            }

            issue.SubtasksDetailed = new List<IssueDetailed>();
            issue.SubtasksDetailed = issue.SubtasksDetailed.Where(s => !s.Entries.IsEmpty()).ToList();
        }

        void SetHasWorkLoggedProperty(IssueDetailed issue)
        {
            if (issue.TimeSpent == 0 && (issue.SubtasksDetailed.IsEmpty()))
                return;

            issue.HasWorkLogged = true;
        }
    }
}
