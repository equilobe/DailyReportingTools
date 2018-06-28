using Equilobe.DailyReport.BL.BitBucket;
using Equilobe.DailyReport.Models;
using Equilobe.DailyReport.Models.BitBucket;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
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
            var client = GetClient(credentials, GetBaseUrl());
            var pullRequests = new List<PullRequest>();
            var page = 1;

            while (true)
            {
                var pullRequestPage = client.GetPullRequests(sourceControlOptions.RepoOwner, sourceControlOptions.Repo, page.ToString());

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
            var client = GetClient(credentials, GetBaseUrl());
            var commits = new List<Commit>();
            var page = 1;

            while (true)
            {
                var commitsPage = client.GetCommits(sourceControlOptions.RepoOwner, sourceControlOptions.Repo, page.ToString());

                if (commitsPage.Values != null)
                    commits.AddRange(commitsPage.Values);

                if (commitsPage.Next == null)
                    break;

                if (IsAnyCommitOutOfRange(commitsPage.Values, fromDate))
                    break;

                page++;
            }

            return ExtractOutOfRangeCommits(commits, fromDate, toDate);
        }

        private bool IsAnyCommitOutOfRange(List<Commit> commits, DateTime fromDate)
        {
            return commits
                .Any(p => p.CreatedAt < fromDate);
        }

        private List<Commit> ExtractOutOfRangeCommits(List<Commit> commits, DateTime fromDate, DateTime toDate)
        {
            commits.RemoveAll(p => p.CreatedAt < fromDate || p.CreatedAt > toDate);

            return commits;
        }

        private BitBucketClient GetClient(Credentials credentials, string baseUrl)
        {
            return BitBucketClient.CreateWithBasicAuth(baseUrl, credentials.Username, credentials.Password);
        }

        private string GetBaseUrl()
        {
            return ConfigurationService.GetBitBucketApiClientUrl();
        }
    }
}
