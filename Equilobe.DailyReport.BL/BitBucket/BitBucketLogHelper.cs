using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.BitBucket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.BL.BitBucket
{
    public static class BitBucketLogHelper
    {
        public static Log LoadLog(List<Commit> commits, List<PullRequest> pullRequests, DateTime fromDate)
        {
            var log = new Log()
            {
                PullRequests = pullRequests.Select(ToOctokitPullRequest).ToList(),
                Entries = commits.Select(GetLogEntry).ToList()
            }; 

            LogHelpers.RemoveWrongEntries(fromDate, log);

            return log;
        }

        private static Octokit.PullRequest ToOctokitPullRequest(PullRequest pullRequest)
        {
            return new Octokit.PullRequest
            {
                HtmlUrl = new Uri(pullRequest.Links.Html.Href),
                Title = pullRequest.Title,
                Number = pullRequest.Id
            };
        }

        private static LogEntry GetLogEntry(Commit commit)
        {
            if (commit.Author == null)
                return null;

            return new LogEntry
            {
                Author = commit.Author.User?.Username,
                Date = Convert.ToDateTime(commit.Date),
                Message = commit.Message,
                Revision = commit.Links.Html.Href,
                Link = commit.Links.Html.Href
            };
        }
    }
}
