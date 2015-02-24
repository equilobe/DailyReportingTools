namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IJiraRequestContext : IRequestContext
    {
        string BaseUrl { get; }
        string SharedSecret { get; }
    }
}
