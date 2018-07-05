using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using Equilobe.DailyReport.Models.SourceControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class SourceControlService : ISourceControlService
    {
        public IGitHubService GitHubService { get; set; }
        public ISvnService SvnService { get; set; }
        public IBitBucketService BitBucketService { get; set; }

        public List<string> GetContributors(SourceControlOptions sourceControlOptions)
        {
            try
            {
                if (sourceControlOptions.Type == SourceControlType.GitHub)
                    return GetGithubContributors(sourceControlOptions);

                if (sourceControlOptions.Type == SourceControlType.SVN)
                    return GetSVNContributors(sourceControlOptions);

                if (sourceControlOptions.Type == SourceControlType.Bitbucket)
                    return GetBitBucketContributors(sourceControlOptions);
            }
            catch (Exception)
            {
            }

            return null;
        }

        private List<string> GetGithubContributors(SourceControlOptions sourceControlOptions)
        {
            return GitHubService.GetAllContributors(sourceControlOptions.Credentials, sourceControlOptions.RepoOwner, sourceControlOptions.Repo)
                .Select(qr => qr.Login)
                .ToList();
        }

        private List<string> GetSVNContributors(SourceControlOptions sourceControlOptions)
        {
            var context = new SourceControlContext
            {
                SourceControlOptions = sourceControlOptions,
                FromDate = DateTime.Now,
                ToDate = DateTime.Now.AddMonths(-3)
            };

            return SvnService.GetAllAuthors(context);
        }

        private List<string> GetBitBucketContributors(SourceControlOptions sourceControlOptions)
        {
            var contributors = BitBucketService.GetAllContributors(sourceControlOptions);

            if (!contributors.Any())
                return GetBitbucketContributorsFromCommits(sourceControlOptions);

            return contributors;
        }

        private List<string> GetBitbucketContributorsFromCommits(SourceControlOptions sourceControlOptions)
        {
            var context = new SourceControlContext
            {
                SourceControlOptions = sourceControlOptions,
                FromDate = DateTime.Now.AddDays(-14),
                ToDate = DateTime.Now
            };

            return BitBucketService.GetContributorsFromCommits(context);
        }
    }
}
