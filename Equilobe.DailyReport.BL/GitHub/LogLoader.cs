using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.SourceControl;
using Equilobe.DailyReport.Utils;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.BL.GitHub
{
    public class LogLoader
    {
        GithubClient Client { get; set; }
        ISourceControlContext Context { get; set; }

        public LogLoader(GithubClient client, ISourceControlContext context)
        {
            Client = client;
            Context = context;
        }

        public Log CreateLog()
        {
            var pullRequests = Client.GetPullRequests(Context.SourceControlOptions.RepoOwner, Context.SourceControlOptions.Repo);
            var commits = GetReportCommits();
            return LoadLog(commits, pullRequests, Context.FromDate);
        }

        protected Log LoadLog(List<GitHubCommit> commits, List<PullRequest> pullRequests, DateTime fromDate)
        {
            var log = new Log();
            if (pullRequests != null)
                log.PullRequests = pullRequests;

            log.Entries = commits.Select(GetLogEntry).ToList();

            LogHelpers.RemoveWrongEntries(fromDate, log);
            return log;
        }

        private static LogEntry GetLogEntry(GitHubCommit commit)
        {
            return new LogEntry
            {
                Author = commit.Commit.Author.Name,
                Date = commit.Commit.Author.Date,
                Message = commit.Commit.Message,
                Revision = commit.Commit.Sha,
                Link = commit.HtmlUrl
            };
        }

        List<GitHubCommit> GetReportCommits()
        {
            string fromDate = TimeFormatting.DateToISO(Context.FromDate);
            string toDate = TimeFormatting.DateToISO(Context.ToDate);
            return Client.GetAllCommits(Context.SourceControlOptions.RepoOwner, Context.SourceControlOptions.Repo, fromDate, toDate);
        }
    }
}
