namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IJiraRequestContext 
    {
        string BaseUrl { get; }
        string SharedSecret { get; }
        string JiraUsername { get; }
        string JiraPassword { get; }
    }
}
