using System;
namespace Equilobe.DailyReport.Models.ReportFrame
{
    public interface IIndividualReport : IJiraReport
    {
        JiraAuthor Author { get; set; }
    }
}
