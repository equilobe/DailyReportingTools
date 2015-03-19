namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface ISvnService : IService
    {
        Log GetLog(ISourceControlContext context, string pathToLog);
    }
}
