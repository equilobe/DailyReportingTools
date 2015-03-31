using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Interfaces;
using Equilobe.DailyReport.Models.Policy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class SourceControlService : ISourceControlService
    {
        public IGitHubService GitHubService { get; set; }

        public List<string> GetContributors(SourceControlOptions sourceControlOptions)
        {
            try
            {
                if (sourceControlOptions.Type == SourceControlType.GitHub)
                    return GitHubService.GetAllContributors(sourceControlOptions.Credentials, sourceControlOptions.RepoOwner, sourceControlOptions.Repo)
                        .Select(qr => qr.Login)
                        .ToList();

                if (sourceControlOptions.Type == SourceControlType.SVN)
                    return new List<string> { "not implemented" };
            }
            catch (Exception)
            {
            }

            return new List<string>();
        }
    }
}
