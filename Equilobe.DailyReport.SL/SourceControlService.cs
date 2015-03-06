using Equilobe.DailyReport.Models.Enums;
using Equilobe.DailyReport.Models.Policy;
using System.Collections.Generic;
using System.Linq;

namespace Equilobe.DailyReport.SL
{
    public class SourceControlService
    {
        public List<string> GetContributors(SourceControlOptions sourceControlOptions)
        {
            if (sourceControlOptions.Type == SourceControlType.GitHub)
                return new GitHubService().GetAllContributors(sourceControlOptions.Credentials, sourceControlOptions.RepoOwner, sourceControlOptions.Repo)
                .Select(qr => qr.Login)
                .ToList();

            if (sourceControlOptions.Type == SourceControlType.SVN)
                return new List<string> { "not implemented" };

            return new List<string>();
        }
    }
}
