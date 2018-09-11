using Equilobe.DailyReport.Models.Policy;
using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IAdvancedSettingsDataService : IService
    {
        List<SourceControlOptions> GetAllReposSourceControlOptions(long instanceId);
        List<User> GetUserMappings(long instanceId);
    }
}
