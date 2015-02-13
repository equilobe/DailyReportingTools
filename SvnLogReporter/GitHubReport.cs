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
            var pullRequests = new GitHubService().GetPullRequests(Policy.SourceControlOptions.Credentials, Policy.SourceControlOptions.RepoOwner, Policy.SourceControlOptions.RepoName);
            var commits = GetReportCommits();
            return LoadLog(commits, pullRequests, Options.FromDate);
        }

        protected Log LoadLog(List<GitHubCommit> commits, List<PullRequest> pullRequests, DateTime fromDate)
        {
            var log = new Log();
            if (pullRequests != null)
                log.PullRequests = pullRequests;

            log.Entries = new List<LogEntry>();
            foreach (var commit in commits)
            {
                log.Entries.Add(
                 new LogEntry
                 {
                     Author = commit.Commit.Author.Name,
                     Date = commit.Commit.Author.Date,
                     Message = commit.Commit.Message,
                     Revision = commit.Sha,
                     Link = commit.HtmlUrl
                 });                
            }

            LogService.RemoveWrongEntries(fromDate, log);
            return log;
        }

        protected override void AddPullRequests(Report report, Log log)
        {
            report.PullRequests = log.PullRequests;
        }

        protected virtual List<GitHubCommit> GetReportCommits()
        {
            string fromDate = Options.DateToISO(Options.FromDate);
            string toDate = Options.DateToISO(Options.ToDate);
            return new GitHubService().GetAllCommits(Policy.SourceControlOptions.Credentials, Policy.SourceControlOptions.RepoOwner, Policy.SourceControlOptions.RepoName, fromDate, toDate);
        }
    }
}
