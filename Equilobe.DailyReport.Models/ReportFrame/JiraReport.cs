using Equilobe.DailyReport.Models.Jira;
using Equilobe.DailyReport.Models.Storage;
using Equilobe.DailyReport.Models.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public IJiraRequestContext Settings { get; set; }

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


        private DateTime? _reportDate;
        public DateTime ReportDate
        {
            get
            {
                if (!_reportDate.HasValue)
                    _reportDate = DateTime.Now.ToOriginalTimeZone(this.OffsetFromUtc);
                return _reportDate.Value;
            }
        }

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

        public DateTime Date { get { return Options.FromDate; } }
        public DateTime FromDate { get { return Options.FromDate; } }
        public DateTime ToDate { get { return Options.ToDate; } }

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

        private Sprint _sprint;
        public Sprint Sprint
        {
            get
            {
                if (_sprint == null)
                    throw new InvalidOperationException("You must first set a value on this property to be able to get it!");
                return _sprint;
            }
            set { _sprint = value; }
        }

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


        private SprintTasks _sprintTasks;
        public SprintTasks SprintTasks
        {
            get
            {
                if (_sprintTasks == null)
                    throw new InvalidOperationException("You must first set a value on this property to be able to get it!");
                return _sprintTasks;
            }
            set { _sprintTasks = value; }
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
                if (!Policy.GeneratedProperties.IsIndividualDraft)
                    throw new InvalidOperationException("This is not an individual report!");

                return _author;
            }
            set
            {
                _author = value;
            }
        }
    }
}
