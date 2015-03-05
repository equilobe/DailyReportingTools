using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Storage;
using Octokit;
using Octokit.Internal;
using RazorEngine;
using SourceControlLogReporter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Equilobe.DailyReport.Models.SourceControl;
using Equilobe.DailyReport.SL;
using Equilobe.DailyReport.Models.Policy;

namespace SourceControlLogReporter
{
    public class GitHubReport : ReportBase
    {
        public GitHubReport(Policy p, Options o):base(p,o)
        {

        }

        public GitHubReport()
        {

        }

        public override Log CreateLog()
        {
            var context = new SourceControlContext { SourceControlOptions = Policy.SourceControlOptions, FromDate = Options.FromDate, ToDate = Options.ToDate };
            return new GitHubService().GetLog(context);
        }

        protected override void AddPullRequests(Report report, Log log)
        {
            report.PullRequests = log.PullRequests;
        }


    }
}
