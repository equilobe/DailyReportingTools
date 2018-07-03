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

        public Log GetLog(ISourceControlContext context)
        {
            var pullRequests = GetAllPullRequests(context.SourceControlOptions);
            var commits = GetAllCommits(context.SourceControlOptions, context.FromDate, context.ToDate);

            return BitBucketLogHelper.LoadLog(commits, pullRequests, context.FromDate);
        }

        public List<PullRequest> GetAllPullRequests(SourceControlOptions sourceControlOptions)
        {
            var credentials = sourceControlOptions.Credentials;
            var client = GetClient(credentials);
            var pullRequests = new List<PullRequest>();
            var page = 1;

            while (true)
            {
                var pullRequestPage = client.GetPullRequests(sourceControlOptions.RepoOwner, sourceControlOptions.Repo, page);

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

        public List<string> GetAllContributors(SourceControlContext context)
        {
            return GetAllCommits(context.SourceControlOptions, context.FromDate, context.ToDate)
                .Where(p => p.Author != null && p.Author.User != null)
                .Select(p => p.Author.User.DisplayName)
                .Distinct()
                .ToList();
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
