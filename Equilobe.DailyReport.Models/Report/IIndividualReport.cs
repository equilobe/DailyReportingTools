using System;
namespace Equilobe.DailyReport.Models.Report
{
    public interface IIndividualReport : IJiraReport
    {
        JiraAuthor Author { get; set; }
    }
}
