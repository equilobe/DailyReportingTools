using Equilobe.DailyReport.Models.ReportFrame;
namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IJiraRequestContext : IRequestContext
    {
        string BaseUrl { get; }
        string SharedSecret { get; }
    }
}
