using Equilobe.DailyReport.BL.BitBucket;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.BitBucket;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.SourceControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class BitBucketService : IBitBucketService
    {
        public IConfigurationService ConfigurationService { get; set; }

        public List<PullRequestComment> GetPullRequestComments(SourceControlOptions options, int pullRequestId, DateTime lastSync)
        {
            var credentials = options.Credentials;
            var client = GetClient(credentials);
            var page = 1;
            var created = lastSync.ToString("yyyy-MM-dd");
            var comments = new List<PullRequestComment>();

            while (true)
            {
                var commentsPage = client.GetPullRequestComments(options.RepoOwner, options.Repo, pullRequestId, created, page);

                if (commentsPage.Values != null)
                    comments.AddRange(commentsPage.Values);

                if (commentsPage.Next == null)
                    break;

                page++;
            }

            return comments;
        }

        public Log GetLog(ISourceControlContext context)
        {
            var pullRequests = GetAllPullRequests(context.SourceControlOptions);
            var commits = GetAllCommits(context.SourceControlOptions, context.FromDate, context.ToDate);

            return BitBucketLogHelper.LoadLog(commits, pullRequests, context.FromDate);
        }

        public List<PullRequest> GetAllPullRequests(SourceControlOptions sourceControlOptions, DateTime? lastSync = null)
        {
            var credentials = sourceControlOptions.Credentials;
            var client = GetClient(credentials);
            var updated = lastSync.HasValue ? lastSync.Value.ToString("yyyy-MM-dd") : null;
            var pullRequests = new List<PullRequest>();
            var page = 1;

            while (true)
            {
                var pullRequestPage = client.GetPullRequests(sourceControlOptions.RepoOwner, sourceControlOptions.Repo, updated, page);

                if (pullRequestPage.Values != null)
                    pullRequests.AddRange(pullRequestPage.Values);

                if (pullRequestPage.Next == null)
                    break;

                page++;
            }

            return pullRequests;
        }

        public List<Commit> GetAllCommits(SourceControlOptions sourceControlOptions, DateTime fromDate, DateTime toDate)
        {
            var credentials = sourceControlOptions.Credentials;
            var client = GetClient(credentials);
            var commits = new List<Commit>();
            var page = 1;

            while (true)
            {
                var commitsPage = client.GetCommits(sourceControlOptions.RepoOwner, sourceControlOptions.Repo, page);

                if (commitsPage.Values != null)
                    commits.AddRange(commitsPage.Values);

                if (commitsPage.Next == null)
                    break;

                if (IsAnyCommitOutdated(commitsPage.Values, fromDate))
                    break;

                page++;
            }

            return commits
                .Where(p => p.CreatedAt >= fromDate && p.CreatedAt <= toDate)
                .ToList();
        }

        public List<string> GetContributorsFromCommits(SourceControlContext context)
        {
            return GetAllCommits(context.SourceControlOptions, context.FromDate, context.ToDate)
                .Where(p => p.Author != null && p.Author.User != null)
                .Select(p => p.Author.User.DisplayName)
                .Distinct()
                .ToList();
        }

        public List<string> GetAllContributors(SourceControlOptions options)
        {
            var credentials = options.Credentials;
            var client = GetClient(credentials);
            var contributors = new List<Contributor>();
            var page = 1;

            while (true)
            {
                var contributorsPage = client.GetContributors(options.RepoOwner, page);

                if (contributorsPage.Values == null)
                    break;

                contributors.AddRange(contributorsPage.Values);

                if (contributorsPage.Next == null)
                    break;

                page++;
            }

            return contributors.Select(p => p.DisplayName).ToList();
        }

        private bool IsAnyCommitOutdated(List<Commit> commits, DateTime fromDate)
        {
            return commits
                .Any(p => p.CreatedAt < fromDate);
        }

        private BitBucketClient GetClient(Credentials credentials)
        {
            return BitBucketClient.CreateWithBasicAuth(GetBaseUrl(), credentials.Username, credentials.Password);
        }

        private string GetBaseUrl()
        {
            return ConfigurationService.GetBitBucketApiClientUrl();
        }
    }
}
