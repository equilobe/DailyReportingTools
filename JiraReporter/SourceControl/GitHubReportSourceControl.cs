using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Storage;
using Octokit;
using Octokit.Internal;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.ReportFrame;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Models.SourceControl;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.Interfaces;

namespace JiraReporter.SourceControl
{
    class GitHubReportSourceControl : GitHubReport
    {
        JiraPolicy Policy { get { return Context.Policy; } }
        JiraOptions Options { get { return Context.Options; } }
        JiraReport Context { get; set; }

        public GitHubReportSourceControl(JiraReport context)
        {
            Context = context;
        }

        public GitHubReportSourceControl()
        {

        }

        public override Log CreateLog()
        {
            var context = new SourceControlContext { SourceControlOptions = Policy.SourceControlOptions, FromDate = Options.FromDate, ToDate = Options.ToDate };
            var log = GitHubService.GetLog(context);
            return log;
        }
    }
}
