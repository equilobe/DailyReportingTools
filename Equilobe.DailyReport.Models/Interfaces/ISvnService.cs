using System.Collections.Generic;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ISvnService : IService
    {
        Log GetLog(ISourceControlContext context);
        Log GetLogWithCommitLinks(ISourceControlContext context);
        List<string> GetAllAuthors(ISourceControlContext context);
    }
}
