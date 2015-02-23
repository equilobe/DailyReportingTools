using Equilobe.DailyReport.Models.ReportFrame;

namespace Equilobe.DailyReport.Models.Interfaces
{
    public interface IIndividualReport : IJiraReport
    {
        JiraAuthor Author { get; set; }
    }
}
