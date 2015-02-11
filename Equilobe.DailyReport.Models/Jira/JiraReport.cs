using Equilobe.DailyReport.Models.ReportPolicy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models.Jira
{
    public class JiraReport
    {
        public JiraPolicy Policy { get; private set; }
        public JiraOptions Options { get; private set; }

        public JiraReport(JiraPolicy p, JiraOptions o)
        {
            Policy = p;
            Options = o;
        }


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
        
        public Summary Summary { get; set; }
        public SprintTasks SprintTasks { get; set; }
        public List<JiraPullRequest> PullRequests { get; set; }
        
        public List<JiraPullRequest> UnrelatedPullRequests
        {
            get
            {
                return PullRequests.FindAll(p => p.TaskSynced == false);
            }
        }

        public bool IsIndividualReport { get; set; }


        private JiraAuthor _author;
        public JiraAuthor Author
        {
            get
            {
                if (!IsIndividualReport)
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
