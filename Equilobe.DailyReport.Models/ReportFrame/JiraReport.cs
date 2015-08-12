using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.IO;

namespace Equilobe.DailyReport.Models.ReportFrame
{
    public class JiraReport : IIndividualReport
    {
        public JiraPolicy Policy { get; private set; }
        public JiraOptions Options { get; private set; }

        public JiraReport(JiraPolicy p, JiraOptions o)
        {
            Policy = p;
            Options = o;
            UniqueProjectKey = Options.UniqueProjectKey;
        }

        public JiraReport() { }

        public BasicSettings Settings { get; set; }

        private TimeSpan? _offsetFromUtc;
        public TimeSpan OffsetFromUtc
        {
            get
            {
                if (_offsetFromUtc == null)
                    throw new InvalidOperationException("You must first set a value on this property to be able to get it!");
                return _offsetFromUtc.Value;
            }
            set
            {
                _offsetFromUtc = value;
            }
        }


        public DateTime ReportDate
        {
            get
            {
                return ToDate;
            }
        }

        public DateTime FullReportDate { get; set; }

        private string _title;
        public string Title
        {
            get
            {
                if (_title == null)
                    throw new InvalidOperationException("You must first set a value on this property to be able to get it!");
                return _title;
            }
            set { _title = value; }
        }

        public string EmailSubject { get; set; }

        private List<JiraAuthor> _authors;
        public List<JiraAuthor> Authors
        {
            get
            {
                if (_authors == null)
                    throw new InvalidOperationException("You must first set a value on this property to be able to get it!");
                return _authors;
            }
            set { _authors = value; }
        }

        public DateTime FromDate { get { return Options.FromDate; } }
        public DateTime ToDate { get { return Options.ToDate; } }
        public string Date { get; set; }

        private Summary _summary;
        public Summary Summary
        {
            get
            {
                if (_summary == null)
                    throw new InvalidOperationException("You must first set a value on this property to be able to get it!");
                return _summary;
            }
            set { _summary = value; }
        }

        public Sprint Sprint { get; set; }
        public Sprint FutureSprint { get; set; }

        private List<JiraCommit> _commits;
        public List<JiraCommit> Commits
        {
            get
            {
                if (_commits == null)
                    throw new InvalidOperationException("You must first set a value on this property to be able to get it!"); return _commits;
            }
            set { _commits = value; }
        }


        private ReportTasks _reportTasks;
        public ReportTasks ReportTasks
        {
            get
            {
                if (_reportTasks == null)
                    throw new InvalidOperationException("You must first set a value on this property to be able to get it!");
                return _reportTasks;
            }
            set { _reportTasks = value; }
        }

        private List<JiraPullRequest> _pullRequests;
        public List<JiraPullRequest> PullRequests
        {
            get
            {
                if (_pullRequests == null)
                    throw new InvalidOperationException("You must first set a value on this property to be able to get it!");
                return _pullRequests;
            }
            set { _pullRequests = value; }
        }


        public List<JiraPullRequest> UnrelatedPullRequests
        {
            get
            {
                return PullRequests.FindAll(p => p.TaskSynced == false);
            }
        }

        private JiraAuthor _author;
        public JiraAuthor Author
        {
            get
            {
                if (!IsIndividualDraft)
                    throw new InvalidOperationException("This is not an individual report!");

                return _author;
            }
            set
            {
                _author = value;
            }
        }

        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }
        public string UniqueProjectKey { get; set; }

        public string NotHideContentId { get; set; }

        public JiraRequestContext JiraRequestContext { get; set; }
        public WorkingDaysContext WorkingDaysContext { get; set; }
        public ExecutionInstance ExecutionInstance { get; set; }

        public DateTime LastReportSentDate { get; set; }
        public DateTime LastDraftSentDate { get; set; }
        public DateTime LastFinalDraftConfirmationDate { get; set; }

        public bool IsFinalDraft { get; set; }
        public bool IsIndividualDraft { get; set; }
        public bool IsFinalReport { get; set; }

        public Uri SendReportUrl { get; set; }
        public Uri SendDraftUrl { get; set; }
        public Uri IndividualDraftConfirmationUrl { get; set; }
        public Uri SendIndividualDraftUrl { get; set; }

        public string ProjectManager { get; set; }

        public string RootPath { get { return Path.GetFullPath(UniqueProjectKey); } }

        public string LogPath { get { return Path.Combine(RootPath, "Logs"); } }
        public string LogArchivePath { get { return Path.Combine(RootPath, "LogArchive"); } }
        public string ReportsPath { get { return Path.Combine(RootPath, "Reports"); } }
        public string UnsentReportsPath { get { return Path.Combine(RootPath, "UnsentReports"); } }

        public bool IsOnSchedule { get; set; }
        public bool HasSprint
        {
            get
            {
                return Sprint != null;
            }
        }

    }
}
