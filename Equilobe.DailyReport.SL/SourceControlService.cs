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

        public List<string> GetContributors(SourceControlOptions sourceControlOptions)
        {
            try
            {
                if (sourceControlOptions.Type == SourceControlType.GitHub)
                    return GitHubService.GetAllContributors(sourceControlOptions.Credentials, sourceControlOptions.RepoOwner, sourceControlOptions.Repo)
                                        .Select(qr => qr.Login)
                                        .ToList();

                if (sourceControlOptions.Type == SourceControlType.SVN)
                {
                    var context = new SourceControlContext
                    {
                        SourceControlOptions = sourceControlOptions,
                        FromDate = DateTime.Now,
                        ToDate = DateTime.Now.AddMonths(-3)
                    };
                    
                    return SvnService.GetAllAuthors(context);
                }
            }
            catch (Exception)
            {
            }

            return new List<string>();
        }
    }
}
