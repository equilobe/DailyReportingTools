using System;
using System.Collections.Generic;
namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ISourceControlService : IService
    {
        List<string> GetContributors(Equilobe.DailyReport.Models.Policy.SourceControlOptions sourceControlOptions);
    }
}
