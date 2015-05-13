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
            dayLog.Title = TimeFormatting.GetStringDay(_date, _context.ReportDate);
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
                dayLog.Issues = TaskLoader.GetParentTasks(dayLog.Issues, _author);
            dayLog.UnsyncedCommits = new List<JiraCommit>(dayLog.Commits.FindAll(c => c.TaskSynced == false));
            dayLog.TimeLogged = dayLog.TimeSpent.SetTimeFormat();

            return dayLog;
        }

        private void AddIssueToDaylog(JiraDayLog dayLog, IssueDetailed issue)
        {
            var currentIssue = new IssueDetailed(issue);
            currentIssue.ExistsInTimesheet = true;
            var entryContext = GetEntryContext(currentIssue);

            IssueAdapter.RemoveWrongEntries(entryContext);
            IssueAdapter.TimeSpentFromEntries(currentIssue);
            IssueAdapter.SetTimeFormat(currentIssue);
            dayLog.Issues.Add(currentIssue);
            dayLog.TimeSpent += currentIssue.TimeSpent;
        }

        private EntryContext GetEntryContext(IssueDetailed currentIssue)
        {
            var entryContext = new EntryContext
            {
                AuthorName = _author.Name,
                FromDate = _date,
                ToDate = _date.AddDays(1),
                Issue = currentIssue,
                OffsetFromUtc = _context.OffsetFromUtc
            };
            return entryContext;
        }
    }
}
