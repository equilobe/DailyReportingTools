
namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ISvnService
    {
        Log GetLog(ISourceControlContext context, string pathToLog);
    }
}
